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
}