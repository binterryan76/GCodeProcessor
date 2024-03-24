using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeParser;

public class GCodeWord
{
    private string text;
    public char Letter { get; set; }
    public int IntValue { get; set; }
    public double DoubleValue { get; set; }
    public bool HasInt { get; set; }
    public bool HasDouble { get; set; }

    public string Text
    {
        get { return text; }
        set
        {
            text = value;
            ParseText();
        }
    }

    public GCodeWord(string text)
    {
        this.text = text;
        ParseText();
    }

    private void ParseText()
    {
        Letter = text[0];
        string numberPart = text.Substring(1);

        int intVal;
        double doubleVal;

        HasInt = int.TryParse(numberPart, out intVal);
        HasDouble = double.TryParse(numberPart, out doubleVal);

        IntValue = HasInt ? intVal : 0;
        DoubleValue = HasDouble ? doubleVal : 0;
    }
}
