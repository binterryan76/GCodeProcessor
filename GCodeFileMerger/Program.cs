using System.Text;
using StringHelpers;
using GCodeParser;
using System.Numerics;
using System.Diagnostics;

/// Change this to match the output directory of Fusion 360
const string FUSION_OUTPUT_DIRECTORY = "C:\\Users\\binte\\Documents\\Fusion 360\\NC Programs\\Temp Output";

ProcessPrograms();

/// This program merges multiple NC programs from fusion 360
/// if Fusion 360 outputs 5 programs because of 5 tool changes
/// then the output program will be composed of header 1, body 1, body 2, body 3, body 4, body 5, footer 5.
/// To find the segments of a single program you can simply split by totally blank lines and it will be divided into the header, body, and footer
static void ProcessPrograms()
{
    Dictionary<string, SortedList<int, string>> filePathGroups = GetFilePathGroups();

    // loop through each group of nc files
    foreach (KeyValuePair<string, SortedList<int, string>> filePathGroupPair in filePathGroups)
    {
        string fileNameWithoutSuffix = filePathGroupPair.Key;
        string outputFilePath = $"{FUSION_OUTPUT_DIRECTORY}\\{fileNameWithoutSuffix}.nc";
        SortedList<int, string> filePathGroup = filePathGroupPair.Value;

        // each group of nc files gets a single output file
        StringBuilder outputFileContents = new();
        uint lineNumber = 10;

        // loop through each nc file in a single group
        foreach (KeyValuePair<int, string> filePathSuffixPair in filePathGroup)
        {
            int fileSuffix = filePathSuffixPair.Key;
            string filePath = filePathSuffixPair.Value;
            FusionGCodeFile gCodeFile = new(filePath);
            
            MoveComments(gCodeFile);
            MergeFirstEHCommands(gCodeFile);

            // append header of first file
            if (fileSuffix == 0)
                foreach (GCodeLine line in gCodeFile.Header)
                    UpdateLineNumberAndAppendGCodeLine(outputFileContents, ref lineNumber, line);

            // append body of every file
            foreach (GCodeLine line in gCodeFile.Body)
                UpdateLineNumberAndAppendGCodeLine(outputFileContents, ref lineNumber, line);

            // append footer of last file
            if (fileSuffix == filePathGroup.Count - 1)
                foreach (GCodeLine line in gCodeFile.Footer)
                    UpdateLineNumberAndAppendGCodeLine(outputFileContents, ref lineNumber, line);
        }

        // write output file for the current group
        File.WriteAllText(outputFilePath, outputFileContents.ToString());
    }

    DeleteFiles(filePathGroups);
}

/// Merges the line with the first H command into the line with the first E command because applying the tool and fixture offsets at different times is dangerous
static void MergeFirstEHCommands(FusionGCodeFile gCodeFile)
{
    // first get the GCodeLines with the first E word and H word
    GCodeLine firstECommandLine = null;
    GCodeLine firstHCommandLine = null;

    foreach (GCodeLine line in gCodeFile.Body)
    {
        foreach(GCodeWord word in line.Words)
        {
            if (word.Letter == 'E')
                firstECommandLine = line;
            else if (word.Letter == 'H')
                firstHCommandLine = line;
        }

        // loop until both lines have been found
        if (firstECommandLine != null && firstHCommandLine != null)
            break;
    }
    
    // then do a bunch of checks for foratting
    Debug.Assert(firstECommandLine != null && firstHCommandLine != null, "Every file should contain an E word and an H word.");

    bool hasUnexpectedWord = false;
    foreach(GCodeWord word in firstECommandLine.Words)
    {
        if (!(
            (word.Letter == 'N' && word.HasInt) // word is properly formatted line number
            || (word.Letter == 'G' && word.HasInt && (word.IntValue == 0 || word.IntValue == 1)) // word is properly formatted G0 or G1
            || (word.Letter == 'E' && word.HasInt) // or word is properly formatted E word
            || (word.Letter == 'X' && word.HasDouble) // or word is properly formatted X word
            || (word.Letter == 'Y' && word.HasDouble) // or word is properly formatted Y word
            || (word.Letter == 'F' && word.HasInt) // or word is properly formatted F word
            ))
        {
            hasUnexpectedWord = true;
        }
    }

    Debug.Assert(!hasUnexpectedWord, "Line with first E word must not contain anything but a line number, G0, G1, E word, X word, or Y word.");

    // for now im only allowing a feedrate F word on the line with the E command and not the line with the H command.
    foreach (GCodeWord word in firstHCommandLine.Words)
    {
        if (!(
            (word.Letter == 'N' && word.HasInt) // word is properly formatted line number
            || (word.Letter == 'H' && word.HasInt) // or word is properly formatted H word
            || (word.Letter == 'Z' && word.HasDouble) // or word is properly formatted Z word
            ))
        {
            hasUnexpectedWord = true;
        }
    }

    Debug.Assert(!hasUnexpectedWord, "Line with first G word must not contain anything but a line number, H word, or Z word.");

    int eCommandLineIndex = gCodeFile.GetLineIndex(firstECommandLine);
    int hCommandLineIndex = gCodeFile.GetLineIndex(firstHCommandLine);

    Debug.Assert(hCommandLineIndex == eCommandLineIndex + 1, "The first H command should come right after the first E command.");

    GCodeWord? nWord = firstECommandLine.GetFirstWordWithLetter('N');
    GCodeWord? gWord = firstECommandLine.GetFirstWordWithLetter('G');
    GCodeWord? eWord = firstECommandLine.GetFirstWordWithLetter('E');
    GCodeWord? xWord = firstECommandLine.GetFirstWordWithLetter('X');
    GCodeWord? yWord = firstECommandLine.GetFirstWordWithLetter('Y');
    GCodeWord? hWord = firstHCommandLine.GetFirstWordWithLetter('H');
    GCodeWord? zWord = firstHCommandLine.GetFirstWordWithLetter('Z');
    GCodeWord? fWord = firstECommandLine.GetFirstWordWithLetter('F');

    Debug.Assert(gWord != null, "G word is missing.");
    Debug.Assert(eWord != null, "E word is missing.");
    Debug.Assert(hWord != null, "H word is missing.");
    Debug.Assert(gWord.HasInt && (gWord.IntValue == 0 || gWord.IntValue == 1), "G word isnt a G0 or G1.");

    // build text for a new GCodeLine
    StringBuilder newLineText = new();
    if (nWord != null)
        newLineText.Append($"{nWord.Text} ");
    newLineText.Append($"{gWord.Text} ");
    newLineText.Append($"{eWord.Text} ");
    if (xWord != null)
        newLineText.Append($"{xWord.Text} ");
    if (yWord != null)
        newLineText.Append($"{yWord.Text} ");
    if (zWord != null)
        newLineText.Append($"{zWord.Text}");
    newLineText.Append($"{hWord.Text} ");
    if (fWord != null)
        newLineText.Append($"{fWord.Text}");


    GCodeWord? nullWord = null;
    GCodeLine newLine = new(newLineText.ToString(), ref nullWord);

    // replace old E command line with new merged line
    gCodeFile.ReplaceLine(eCommandLineIndex, newLine);

    // remove H command line since it is merged with E command line
    gCodeFile.RemoveLine(hCommandLineIndex);
}

/// Moves the tool description comment right after the first operation description comment
static void MoveComments(FusionGCodeFile gCodeFile)
{
    Debug.Assert(gCodeFile.ToolDescriptionComments.Count == 1, "There should only be one tool description comment per file.");
    int toolDescriptionLineIndex = gCodeFile.GetLineIndex(gCodeFile.ToolDescriptionComments[0]);
    int firstOperationLineIndex = gCodeFile.GetLineIndex(gCodeFile.OtherFullLineComments[0]);

    // put the tool description comment right after the first operation description comment
    gCodeFile.ReorderLine(toolDescriptionLineIndex, firstOperationLineIndex + 1);
}

/// Updates the line number of the GCodeLine and increments the lineNumber by 10 if there is a line number.
/// Adds the new GCodeLine text to the outputFileContents.
static void UpdateLineNumberAndAppendGCodeLine(StringBuilder outputFileContents, ref uint lineNumber, GCodeLine line)
{
    if (line.LineNumberWord != null)
    {
        line.LineNumberWord.IntValue = (int)lineNumber;
        lineNumber += 10;
    }
    outputFileContents.AppendLine(line.UpdatedText);
}

/// Deletes all the files created by Fusion 360
static void DeleteFiles(Dictionary<string, SortedList<int, string>> filePathGroups)
{
    // loop through each group of nc files
    foreach (SortedList<int, string> filePathGroup in filePathGroups.Values)
    {
        // loop through each nc file in a single group
        foreach (string filePath in filePathGroup.Values)
        {
            File.Delete(filePath);
        }
    }
}

/// This will look at all the files in the output directory for Fusion 360 and group them into many lists where each list of file names will be merged into a single program. 
/// The key for each list will be the NC program name which is the base string before the dash at the end.
static Dictionary<string, SortedList<int, string>> GetFilePathGroups()
{
    Dictionary<string, SortedList<int, string>> filePathGroups = new();

    foreach (string filePath in Directory.GetFiles(FUSION_OUTPUT_DIRECTORY))
    {
        // every nc file should have the correct file extension and have a separator between the file name and the suffix
        if (!(filePath.EndsWith(".nc") && filePath.Contains('-')))
            continue;

        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string fileNameWithoutSuffix = fileName.RemoveNumbersFromEnd().TrimEnd('-');
        int fileSuffix = int.Parse(fileName.GetNumbersFromEnd());

        // make sure the dictionary has a list for the current fileNameWithoutSuffix
        if (!filePathGroups.ContainsKey(fileNameWithoutSuffix))
            filePathGroups[fileNameWithoutSuffix] = new SortedList<int, string>();

        // add the filePath to the list in order based on the file suffix
        filePathGroups[fileNameWithoutSuffix].Add(fileSuffix, filePath);
    }

    return filePathGroups;
}