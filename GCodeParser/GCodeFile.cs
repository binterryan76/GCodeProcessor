using StringHelpers;
using System.Collections.Generic;
using System;
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

    /// <summary>
    /// Moves the GCodeLine from fromIndex to come before the element at toIndex.
    /// Note: There is no difference between moving the element at index 1 to come before the element at index 1 or to come before the element at index 2.
    /// </summary>
    /// <param name="fromIndex"></param>
    /// <param name="toIndex"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public virtual void ReorderLine(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= Lines.Count)
            throw new ArgumentOutOfRangeException(nameof(fromIndex));

        if (toIndex < 0 || toIndex > Lines.Count)
            throw new ArgumentOutOfRangeException(nameof(toIndex));

        // If fromIndex == toIndex or toIndex is one more than fromIndex, then nothing needs moved
        if (fromIndex == toIndex || (fromIndex + 1) == toIndex)
            return;

        GCodeLine item = Lines[fromIndex];
        Lines.RemoveAt(fromIndex);

        // Adjust toIndex if necessary because removing shifts the elements
        if (toIndex > fromIndex)
            toIndex--;

        Lines.Insert(toIndex, item);
    }

    /// <summary>
    /// Returns the index of the given GCodeLine.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public int GetLineIndex(GCodeLine line)
    {
        return Lines.IndexOf(line);
    }
}