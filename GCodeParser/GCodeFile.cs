using StringHelpers;
using System.Text;

namespace GCodeParser;

public class GCodeFile
{
    public string FilePath { get; set; }
    public List<GCodeLine> Lines { get; private set; }
    
    public GCodeFile(string filePath)
    {
        FilePath = filePath;
        Parse();
    }

    /// <summary>
    /// Parses the GCode file from the file name.
    /// </summary>
    /// <exception cref="FileNotFoundException"></exception>
    public void Parse()
    {
        if (FilePath.IsNullOrWhitespace() || !File.Exists(FilePath))
            throw new FileNotFoundException("File not found.", FilePath);

        string[] lines = File.ReadAllLines(FilePath);

        Parse(lines);
    }

    /// <summary>
    /// Parses the GCode file from an array of strings.
    /// </summary>
    /// <param name="lines"></param>
    public void Parse(string[] lines)
    {
        Lines = new List<GCodeLine>();

        // this stores the latest command which is needed to determine what future words mean
        // this will be passed by reference and updated as G or M words are encountered
        GCodeWord? latestCommandWord = null;

        foreach (string line in lines)
            Lines.Add(new GCodeLine(line, ref latestCommandWord));
    }

    /// <summary>
    /// Applies comments to each line of the file describing what that line does.
    /// </summary>
    public void AppendComments()
    {
        int maxLineLength = GetMaxLineLength();

        foreach (GCodeLine line in Lines)
        {
            string comment = line.Description.Value;

            // dont add blank descriptions as comments
            if (comment.IsNullOrWhitespace())
                continue;

            line.AppendComment(comment, maxLineLength);
        }
    }

    private int GetMaxLineLength()
    {
        GCodeLine? maxLengthLine = Lines.MaxBy(line => line.Text.Length);
        return maxLengthLine?.Text.Length ?? 0;
    }

    public void SaveAs(string newFilePath)
    {
        StringBuilder output = new();

        foreach (GCodeLine line in Lines)
            output.AppendLine(line.Text);

        File.WriteAllText(newFilePath, output.ToString());
    }
}