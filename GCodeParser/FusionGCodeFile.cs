using StringHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachedProperty;

namespace GCodeParser;

/// <summary>
/// This is a version of a GCodeFile specifically for Fusion 360 NC file outputs 
/// because as far as I know, Fusion 360 always outputs programs in three sections 
/// separated by totally blank lines which I am calling the Header, Body, and Footer.
/// </summary>
public class FusionGCodeFile: GCodeFile
{
    public List<GCodeLine> Header { get; private set; }
    public List<GCodeLine> Body { get; private set; }
    public List<GCodeLine> Footer { get; private set; }
    public List<GCodeLine> ToolDescriptionComments { get; private set; }

    /// <summary>
    /// For Fusion 360 NC files, the only other full comment lines seems to be comments on operations
    /// </summary>
    public List<GCodeLine> OtherFullLineComments { get; private set; }

    public FusionGCodeFile(string filePath) : base(filePath)
    {
        ParseHeaderBodyFooter();
    }

    private void ParseHeaderBodyFooter()
    {
        Header = new List<GCodeLine>();
        Body = new List<GCodeLine>();
        Footer = new List<GCodeLine>();
        ToolDescriptionComments = new List<GCodeLine>();
        OtherFullLineComments = new List<GCodeLine>();

        bool headerComplete = false;
        bool bodyComplete = false;

        foreach (GCodeLine line in Lines)
        {
            // add comments
            if (line.Words.Count < 1 && line.Comments.Count > 0)
            {
                if (line.Comments[0].StartsWith("(T"))
                    ToolDescriptionComments.Add(line);
                else
                    OtherFullLineComments.Add(line);
            }

            // add header lines
            if (!headerComplete)
            {
                Header.Add(line);

                if (line.Text.IsNullOrWhitespace())
                {
                    headerComplete = true;
                    continue;
                }
            }

            // add body lines
            if (headerComplete && !bodyComplete)
            {
                Body.Add(line);

                if (line.Text.IsNullOrWhitespace())
                {
                    bodyComplete = true;
                    continue;
                }
            }

            // add footer lines
            if (headerComplete && bodyComplete)
            {
                Footer.Add(line);
            }
        }
    }

    /// <summary>
    /// Moves the GCodeLine from fromIndex to come before the element at toIndex.
    /// Note: There is no difference between moving the element at index 1 to come before the element at index 1 or to come before the element at index 2.
    /// </summary>
    /// <param name="fromIndex"></param>
    /// <param name="toIndex"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public override void ReorderLine(int fromIndex, int toIndex)
    {
        base.ReorderLine(fromIndex, toIndex);
        ParseHeaderBodyFooter();
    }

    /// <summary>
    /// Replaces the GCodeLine at the given index with the provided newLine.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="newLine"></param>
    public void ReplaceLine(int index, GCodeLine newLine)
    {
        Lines[index] = newLine;
        ParseHeaderBodyFooter();
    }

    /// <summary>
    /// Removes the GCodeLine at the given index.
    /// </summary>
    /// <param name="index"></param>
    public void RemoveLine(int index)
    {
        Lines.RemoveAt(index);
        ParseHeaderBodyFooter();
    }
}
