using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
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
                if (IsValidIntWord(fixtureOffset))
                    return $"RAPID WITH FIXTURE OFFSET {fixtureOffset.IntValue}";
                return "RAPID";
            case 1: return "MOVE";
            case 2: return "CLOCKWISE ARC";
            case 3: return "COUNTERCLOCKWISE ARC";
            case 4: return "DWELL";
            case 5: return "NON MODAL RAPID";
            case 8: return "NO ACCELERATION/FEED RAMP FOR SPEED";
            case 9: return "ACCELERATION/FEED RAMP FOR ACCURACY";
            case 10: return "PROGRAMMABLE DATA INPUT";
            case 15: return "YZ CIRCULAR INTERPOLATION WITH A AXIS";
            case 17: return "USE XY PLANE FOR ARCS, COMPENSATION AND, COORDINATE ROTATIONS";
            case 18: return "USE ZX PLANE FOR ARCS, COMPENSATION AND, COORDINATE ROTATIONS";
            case 19: return "USE YZ PLANE FOR ARCS, COMPENSATION AND, COORDINATE ROTATIONS";
            case 20: return "VERIFY INCH MODE";
            case 21: return "VERIFY METRIC MODE";
            case 28: return "RETURN TO COORDINATE SYTEM ZERO";
            case 29: return "RETURN FROM ZERO";
            case 31: return "PROBE TOUCH FUNCTION";
            case 40: return "CUTTER COMPENSATION CANCEL";
            case 41: return "CUTTER COMPENSATION LEFT";
            case 42: return "CUTTER COMPENSATION RIGHT";
            case 43: return "CUTTER COMPENSATION POSITIVE";
            case 44: return "CUTTER COMPENSATION NEGATIVE";
            case 45: return "TOOL OFFSET SINGLE EXPANSION";
            case 46: return "TOOL OFFSET SINGLE REDUCTION";
            case 47: return "TOOL OFFSET DOUBLE EXPANSION";
            case 48: return "TOOL OFFSET DOUBLE REDUCTION";
            case 49: return "TOOL LENGTH OFFSET CANCEL";
            case 50: return "RAMP CONTROL CANCEL";
            case 51: return "RAMP CONTROL";
            case 52: return "COORDINATE SYSTEM SHIFT";
            case 53: return "USE MACHINE TOOL COORDINATE SYSTEM";
            case 54: return "USE FIXTURE OFFSET 1";
            case 55: return "USE FIXTURE OFFSET 2";
            case 56: return "USE FIXTURE OFFSET 3";
            case 57: return "USE FIXTURE OFFSET 4";
            case 58: return "USE FIXTURE OFFSET 5";
            case 59: return "USE FIXTURE OFFSET 6";
            case 66: return "MODAL SUBROUTINE";
            case 67: return "CANCEL MODAL SUBROUTINE";
            case 68: return "COORDINATE SYSTEM ROTATION";
            case 69: return "COORDINATE SYSTEM ROTATION CANCEL";
            case 70: return "VERIFY INCH MODE";
            case 71: return "VERIFY METRIC MODE";
            case 73: return "FIXED CYCLE";
            case 74: return "FIXED CYCLE";
            case 75: return "FIXED CYCLE";
            case 76: return "FIXED CYCLE";
            case 80: return "FIXED CYCLE CANCEL";
            case 81: return "FIXED CYCLE";
            case 82: return "FIXED CYCLE";
            case 83: return "FIXED CYCLE";
            case 84: return "FIXED CYCLE";
            case 85: return "FIXED CYCLE";
            case 86: return "FIXED CYCLE";
            case 87: return "FIXED CYCLE";
            case 88: return "FIXED CYCLE";
            case 89: return "FIXED CYCLE";
            case 90: return "ABSOLUTE POSITIONING";
            case 91: return "INCREMENTAL POSITIONING";
            case 92: return "SET TEMPORARY COORDINATE SYSTEM";
            case 93: return "USE INVERSE TIME FEEDRADE";
            case 94: return "USE FEEDRATE PER MINUTE";
            case 99: return "RETURN TO INITIAL PLANE";
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
            case 0: return "PROGRAM STOP";
            case 1: return "OPTIONAL PROGRAM STOP";
            case 2: return "END OF PROGRAM";
            case 3: return "SPINDLE CLOCKWISE";
            case 4: return "SPINDLE COUNTERCLOCKWISE";
            case 6: return "CHANGE TOOL";
            case 7: return "MIST COOLANT ON";
            case 8: return "FLOOD COOLANT ON";
            case 9: return "COOLANT OFF";
            case 10: return "CANCEL RECIPROCATION";
            case 11: return "X AXIS RECIPROCATION";
            case 12: return "Y AXIS RECIPROCATION";
            case 13: return "Z AXIS RECIPROCATION";
            case 14: return "B AXIS RECIPROCATION";
            case 15: return "A AXIS RECIPROCATION";
            case 16: return "";
            case 17: return "END OF SUBROUTINE";
            case 18: return "AIR RATCHETING INDEXER";
            case 19: return "SPINDLE STOP AND ORIENT";
            case 20: return "GENERAL PURPOSE INDEXER";
            case 30: return "END OF FILE/SUBROUTINE";
            case 31: return "EXCHANGE PALLETS";
            case 32: return "LOAD AND STORE PALLET A";
            case 33: return "LOAD AND STORE PALLET A";
            case 41: return "BELT DRIVE RANGE 150-2700 RPM";
            case 42: return "BELT DRIVE RANGE 150-5200 RPM";
            case 43: return "BELT DRIVE RANGE 300-10000 RPM";
            case 45: return "EXECUTE FIXED CYCLE";
            case 46: return "POSITIVE APPROACH";
            case 47: return "CANCEL POSITIVE APPROACH";
            case 48: return "POTENTIOMETER CONTROLS IN";
            case 49: return "POTENTIOMETER CONTROLS OUT";
            case -60: return "FIXED CYCLE";
            case -61: return "FIXED CYCLE";
            case -62: return "FIXED CYCLE";
            case 63: return "";
            case 64: return "ACTIVATE MP8 PROBE";
            case 65: return "ACTIVATE TS-20 OR TS-27 TOOL SETTER";
            case 66: return "ACTIVATE MP12 OR MP11 PROBE";
            case 67: return "ACTIVATE LASER PROBE";
            case 80: return "AUTOMATIC DOORS OPEN";
            case 81: return "AUTOMATIC DOORS CLOSE";
            case 90: return "SET DEFAULT GAIN BASED ON SV COMMAND";
            case 91: return "SET NORMAL GAIN FOR < 50 IMP";
            case 92: return "SET INTERMEDIATE GAIN FOR CLOSER TRACKING";
            case 93: return "SET HIGH GAIN FOR RIGID TAPPING CYCLE";
            case 94: return "FEED FORWARD FUNCTION";
            case 95: return "FEED FORWARD CANCEL";
            case 96: return "INTERSECTIONAL CUTTER COMPENSATION CANCELED";
            case 97: return "INTERSECTIONAL CUTTER COMPENSATION";
            case 98:
                GCodeWord? programNumber = GetDependentWord('P');
                GCodeWord? callCount = GetDependentWord('L');

                if (IsValidIntWord(programNumber) && IsValidIntWord(callCount))
                    return $"CALL SUBPROGRAM {programNumber.IntValue} {callCount.IntValue} TIMES";
                if (IsValidIntWord(programNumber))
                    return $"CALL SUBPROGRAM {programNumber.IntValue}";
                if (IsValidIntWord(callCount))
                    return $"CALL SUBPROGRAM ?? {callCount.IntValue} TIMES";

                return "CALL SUBPROGRAM";

            case 99:
                GCodeWord? lineNumber = GetDependentWord('P');

                if (IsValidIntWord(lineNumber))
                    return $"LINE JUMP TO {lineNumber.IntValue}";

                return "END OF SUBPROGRAM";

            default: return $"UNKNOWN M{IntValue} COMMAND";
        }
    }

    /// <summary>
    /// Returns true if GCodeWord? is not null and has an int value.
    /// </summary>
    /// <param name="gCodeWord"></param>
    /// <returns></returns>
    private bool IsValidIntWord(GCodeWord? gCodeWord)
    {
        return gCodeWord != null && gCodeWord.HasInt;
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
