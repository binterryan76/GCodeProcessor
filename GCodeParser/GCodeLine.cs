using CachedProperty;
using StringHelpers;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeParser;

public class GCodeLine
{
    /// <summary>
    /// Warning: setting the text to a new value by causes
    /// the entire file to need to be reparsed because the
    /// words list and Comments will be wrong and adding a
    /// word will cause the latestCommandWord property to
    /// need updating.
    /// </summary>
    public string Text { get; set; }

    private string textWithoutComments;
    public List<GCodeWord> Words { get; private set; }
    public List<string> Comments { get; private set; }
    public CachedProperty<string> Description { get; }
    private GCodeWord? latestCommandWord;

    public GCodeLine(string text, ref GCodeWord? latestCommandWord)
    {
        Text = text;
        Description = new CachedProperty<string>("", GetDescription);
        this.latestCommandWord = latestCommandWord;
        Parse();
    }

    private void Parse()
    {
        textWithoutComments = string.Empty;
        Words = new List<GCodeWord>();
        Comments = new List<string>();

        ParseComments();
        ParseWords(ref latestCommandWord);
    }

    /// <summary>
    /// Fills the Comments list and sets the textWithoutComments variable so words can be parsed.
    /// Everything after semicolon is a comment.
    /// Everything in parentheses is a comment. I dont think GCode matches opening parentheses to closing parentheses.
    /// </summary>
    private void ParseComments()
    {
        bool inComment = false;
        bool everythingElseIsComment = false;

        StringBuilder currentComment = new();
        StringBuilder textWithoutComments = new();

        foreach (char c in Text)
        {
            if (c == '(')
                inComment = true;

            if (c == ';')
            {
                inComment = true;
                everythingElseIsComment = true;
            }

            if (inComment)
                currentComment.Append(c);
            else
                textWithoutComments.Append(c);

            if (c == ')' && !everythingElseIsComment)
            {
                inComment = false;
                Comments.Add(currentComment.ToString());
                currentComment = new StringBuilder();
            }
        }

        // append final comment
        if (currentComment.Length > 0)
            Comments.Add(currentComment.ToString());

        this.textWithoutComments = textWithoutComments.ToString();
    }

    /// <summary>
    /// Fills the Words list using textWithoutComments.
    /// </summary>
    private void ParseWords(ref GCodeWord? latestCommandWord)
    {
        foreach (string word in GetWordsFromText())
        {
            if (word.IsNullOrWhitespace())
                continue;

            Words.Add(new GCodeWord(word, ref latestCommandWord));
        }
    }

    private readonly static string[] MetaCommandChars = new[] { "IF", "GOTO", "=", "#", "[", "]" };
    public bool HasMetaCommands
    {
        get
        {
            string textUpper = textWithoutComments.ToUpper();
            return MetaCommandChars.Any(c => textUpper.Contains(c));
        }
    }

    /// <summary>
    /// Returns a list of gcode words from the gcode text without any comments.
    /// G1M17M20 G30 => G1, M17, M20, G30
    /// </summary>
    /// <param name="textWithoutComments"></param>
    /// <returns></returns>
    private List<string> GetWordsFromText()
    {
        const char BLOCK_DELETE_CHAR = '/';
        List<string> words = new();
        StringBuilder currentWord = new();

        if (textWithoutComments.StartsWith(BLOCK_DELETE_CHAR))
            return words;

        // check for meta commands
        if (HasMetaCommands)
        {
            // if the line has meta commands then it will just be maybe a line number then the rest will just be treated as a single word
            // for now just treat the whole thing as one long weird word i guess...
            words.Add(textWithoutComments);
            return words;
        }

        foreach (char c in textWithoutComments)
        {
            // new letters or whitespace characters indicate the start of a new word
            if ((char.IsLetter(c) || char.IsWhiteSpace(c)) && currentWord.Length > 0)
            {
                if (currentWord.Length == 1)
                    Debug.Assert(false);

                words.Add(currentWord.ToString());
                currentWord.Clear();
            }

            // add all non-whitespace characters to gcode words
            if (!char.IsWhiteSpace(c))
                currentWord.Append(c);
        }

        // make sure last word in substring is added to the list of words
        if (currentWord.Length > 0)
            words.Add(currentWord.ToString());

        return words;
    }

    /// <summary>
    /// Gets a description of an entire line of GCode.
    /// </summary>
    /// <returns></returns>
    private string GetDescription()
    {
        StringBuilder description = new();
        bool first = true;

        foreach (GCodeWord word in Words)
        {
            string wordDescription = word.Description.Value;

            // skip words with blank descriptions
            if (wordDescription.IsNullOrWhitespace())
                continue;

            if (!first)
                description.Append(" | ");

            description.Append(wordDescription);

            first = false;
        }

        return description.ToString();
    }

    /// <summary>
    /// Applies a given comment to the end of this line.
    /// Starts the comment at maxLineLength if provided.
    /// </summary>
    /// <param name="comment"></param>
    /// <param name="maxLineLength"></param>
    public void AppendComment(string comment, int maxLineLength = 0)
    {
        string formattedComment = " ; " + comment;
        string newLine;

        if (maxLineLength > 0)
            newLine = Text.PadRight(maxLineLength) + formattedComment;
        else
            newLine = Text + formattedComment;

        Text = newLine;
        Comments.Add(formattedComment);
    }
}