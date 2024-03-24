using StringHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GCodeParser;

public class GCodeLine
{
    public string Text { get; }
    private string textWithoutComments;
    //public List<GCodeCommand> Commands { get; }
    public List<GCodeWord> Words { get; }
    public List<string> Comments { get; }

    public GCodeLine(string text)
    {
        Text= text;
        textWithoutComments = string.Empty;
        Words = new List<GCodeWord>();
        Comments = new List<string>();

        ParseComments();
        ParseWords();
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
            if(c == '(')
                inComment = true;

            if(c == ';')
            {
                inComment = true;
                everythingElseIsComment = true;
            }

            if(inComment)
                currentComment.Append(c);
            else
                textWithoutComments.Append(c);

            if(c == ')' && !everythingElseIsComment)
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
    private void ParseWords()
    {
        string[] words = textWithoutComments.Split(' ');

        foreach (string word in words)
            Words.Add(new GCodeWord(word));
    }
}
