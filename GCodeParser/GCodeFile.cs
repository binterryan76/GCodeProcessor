using StringHelpers;

namespace GCodeParser;

public class GCodeFile
{
    public string FilePath { get; set; }
    public List<GCodeLine> Lines { get; }
    
    public GCodeFile(string filePath)
    {
        FilePath = filePath;
        Lines = new List<GCodeLine>();
        Parse();
    }

    public void Parse()
    {
        if (FilePath.IsNullOrWhitespace() || !File.Exists(FilePath))
            throw new FileNotFoundException("File not found.", FilePath);

        string[] lines = File.ReadAllLines(FilePath);

        foreach (string line in lines) 
            Lines.Add(new GCodeLine(line));
    }
}