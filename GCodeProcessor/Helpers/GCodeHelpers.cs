using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StringHelpers;

namespace GCodeProcessor.Helpers;

internal static class GCodeHelpers
{
    /// <summary>
    /// Returns a StringBuilder with comments applied to each line of the file.
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    public static StringBuilder GetCommentedFile(string[] lines)
    {
        StringBuilder output = new StringBuilder();

        int maxLineLength = lines.MaxLength();

        foreach (string line in lines)
        {
            string formattedLine = line.Trim().ToUpper();

            if (formattedLine.IsNullOrWhitespace() || formattedLine.ShouldOmitComment())
            {
                // keep empty lines unchanged, comments, or section labels
                output.AppendLine(line);
                continue;
            }

            StringBuilder fullComment = GetFullComment(formattedLine);

            if (fullComment.Length > 0)
                output.AppendLine(line.PadRight(maxLineLength + 1) + fullComment.ToString());
            else
                output.AppendLine(line);
        }

        return output;
    }

    /// <summary>
    /// Returns a StringBuilder containing a comment describing the entire line of code.
    /// </summary>
    /// <param name="formattedLine"></param>
    /// <returns></returns>
    private static StringBuilder GetFullComment(string formattedLine)
    {
        StringBuilder fullComment = new StringBuilder();
        string[] words = formattedLine.Split(' ');
        bool firstComment = true;

        foreach (string word in words)
        {
            // skip any blank words
            if (word.IsNullOrWhitespace())
                continue;

            string comment = GetComment(word);

            if (!comment.IsNullOrWhitespace())
            {
                // first comment gets semicolon to denote a comment, additional comments get delimiter to separate comments
                if (firstComment)
                    fullComment.Append(" ; ");
                else
                    fullComment.Append(" | ");

                fullComment.Append(comment);

                firstComment = false;
            }
        }

        return fullComment;
    }

    /// <summary>
    /// Returns the comment for a single G command.
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    private static string GetGCodeComment(int num)
    {
        switch (num)
        {
            case 0: return "RAPID";
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
        }

        return $"UNKNOWN G{num} COMMAND";
    }

    /// <summary>
    /// Returns the comment for a single M command.
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    private static string GetMCodeComment(int num)
    {
        switch (num)
        {
            case 3: return "SPINDLE CLOCKWISE";
            case 4: return "SPINDLE COUNTERCLOCKWISE";
            case 6: return "CHANGE TOOL";
            case 7: return "COOLANT ON";
            case 9: return "COOLANT OFF";
            case 30: return "END OF FILE/SUBROUTINE";
        }

        return $"UNKNOWN M{num} COMMAND";
    }

    /// <summary>
    /// Returns the comment for the entire line of NC code.
    /// Assumes word is not null or whitespace.
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    private static string GetComment(string word)
    {
        char first = word[0];

        switch (first)
        {
            case 'T':
                return "SET TOOL";
            case 'S':
                return "SET SPINDLE RPM";
            case 'G':
            case 'M':
            {
                bool hasNum = int.TryParse(word.Substring(1), out int num);

                if (!hasNum)
                    return "";

                if (first == 'G')
                    return GetGCodeComment(num);

                if (first == 'M')
                    return GetMCodeComment(num);

                break;
            }
        }

        return "";
    }

    /// <summary>
    /// Returns true if the line of NC code doesn't need a comment.
    /// Assumes line is not empty or null.
    /// </summary>
    /// <param name="gCodeLine"></param>
    /// <returns></returns>
    public static bool ShouldOmitComment(this string gCodeLine)
    {
        char first = gCodeLine[0];
        return first == '(' || first == '%' || first == 'O';
    }
}
