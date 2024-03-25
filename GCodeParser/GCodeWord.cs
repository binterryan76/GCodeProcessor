using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachedProperty;

namespace GCodeParser;

public class GCodeWord
{
    private string text;
    public char Letter { get; set; }
    public int IntValue { get; set; }
    public double DoubleValue { get; set; }
    public bool HasInt { get; set; }
    public bool HasDouble { get; set; }
    private GCodeWord? latestCommandWord;
    public CachedProperty<string> Description { get; private set; }

    /// <summary>
    /// List of GCodeWords that have a latestCommandWord of this word.
    /// </summary>
    private List<GCodeWord> DependantWords { get; }

    public string Text
    {
        get { return text; }
        set
        {
            text = value;
            ParseText();
            GetDescription();
            Description.NeedsUpdate = true; // description needs recomputed if text changes
        }
    }

    public GCodeWord(string text, ref GCodeWord? latestCommandWord)
    {
        this.text = text;
        Description = new CachedProperty<string>(string.Empty, GetDescription);
        DependantWords = new List<GCodeWord>();

        // record the fact that this word depends on the latestCommandWord
        latestCommandWord?.DependantWords.Add(this);
        this.latestCommandWord = latestCommandWord;

        ParseText();

        // update latestCommandWord so that the next word knows this word was the latest G or M code
        if (Letter == 'G' || Letter == 'M')
            latestCommandWord = this;
    }

    private void ParseText()
    {
        Letter = text[0];
        string numberPart = text.Substring(1);

        HasInt = int.TryParse(numberPart, out int intVal);
        HasDouble = double.TryParse(numberPart, out double doubleVal);

        IntValue = HasInt ? intVal : 0;
        DoubleValue = HasDouble ? doubleVal : 0;
    }

    /// <summary>
    /// Returns a dependent word that starts with the given letter or null if it does not exist.
    /// </summary>
    /// <param name="wordLetterToFind"></param>
    /// <returns></returns>
    public GCodeWord? GetDependentWord(char wordLetterToFind)
    {
        foreach (GCodeWord word in DependantWords)
            if (word.Letter == wordLetterToFind)
                return word;

        return null;
    }

    /// <summary>
    /// Returns the description for a single G command.
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    private string GetGCodeDescription()
    {
        switch (IntValue)
        {
            case 0:
                GCodeWord? fixtureOffset = GetDependentWord('E');
                if (fixtureOffset != null && fixtureOffset.HasInt)
                    return $"RAPID WITH FIXTURE OFFSET {fixtureOffset.IntValue}";
                return "RAPID";
            case 1: return "MOVE";
            case 2: return "CLOCKWISE ARC";
            case 3: return "COUNTERCLOCKWISE ARC";
            case 4: return "DWELL";
            case 90: return "ABSOLUTE POSITIONING";
            case 94: return "USE FEEDRATE PER MINUTE";
            case 17: return "USE XY PLANE FOR ARCS, COMPENSATION AND, COORDINATE ROTATIONS";
            case 18: return "USE ZX PLANE FOR ARCS, COMPENSATION AND, COORDINATE ROTATIONS";
            case 19: return "USE YZ PLANE FOR ARCS, COMPENSATION AND, COORDINATE ROTATIONS";
            case 20: return "ENSURES INCH MODE";
            default: return $"UNKNOWN G{IntValue} COMMAND";
        }
    }

    /// <summary>
    /// Returns the description for a single M command.
    /// </summary>
    /// <returns></returns>
    private string GetMCodeDescription()
    {
        switch (IntValue)
        {
            case 3: return "SPINDLE CLOCKWISE";
            case 4: return "SPINDLE COUNTERCLOCKWISE";
            case 6: return "CHANGE TOOL";
            case 7: return "COOLANT ON";
            case 9: return "COOLANT OFF";
            case 30: return "END OF FILE/SUBROUTINE";
            default: return $"UNKNOWN M{IntValue} COMMAND";
        }
    }

    /// <summary>
    /// Returns the comment for the entire line of NC code.
    /// Assumes word is not null or whitespace.
    /// </summary>
    /// <returns></returns>
    private string GetDescription()
    {
        switch (Letter)
        {
            case 'T': return "SET TOOL";
            case 'S': return "SET SPINDLE RPM";
            case 'G': return HasInt ? GetGCodeDescription() : "";
            case 'M': return HasInt ? GetMCodeDescription() : "";
            default: return "";
        }
    }

    /// <summary>
    /// Returns true if word is G0.
    /// </summary>
    public bool IsRapid
    {
        get
        {
            return Letter == 'G' && HasInt && IntValue == 0;
        }
    }

    /// <summary>
    /// Returns true if word is a G or M word.
    /// </summary>
    public bool IsGOrMWord
    {
        get
        {
            return Letter == 'G' || Letter == 'M';
        }
    }
}
