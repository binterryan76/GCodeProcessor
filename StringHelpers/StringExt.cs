using System.Text;

namespace StringHelpers;

public static class StringExt
{
    public static bool IsNullOrWhitespace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    /// <summary>
    /// Returns the maximum length of all the given strings
    /// </summary>
    /// <param name="strs"></param>
    /// <returns></returns>
    public static int MaxLength(this string[] strs)
    {
        int maxLineLength = 0;

        foreach (string line in strs)
            maxLineLength = Math.Max(maxLineLength, line.Length);

        return maxLineLength;
    }

    /// <summary>
    /// Trims all numbers from the end of a string.
    /// Doesn't take decimals into consideration.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string RemoveNumbersFromEnd(this string str)
    {
        return str.TrimEnd(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']);
    }

    /// <summary>
    /// Gets the numbers at the end of a string.
    /// Doesn't take decimals into consideration.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string GetNumbersFromEnd(this string str)
    {
        StringBuilder numbers = new();

        for (int i = str.Length - 1; i >= 0; i--)
            if (char.IsDigit(str[i]))
                numbers.Insert(0, str[i]);
            else
                break;
        
        return numbers.ToString();
    } 
}