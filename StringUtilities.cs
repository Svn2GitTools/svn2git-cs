namespace Svn2GitConsole;

public static class StringUtilities
{
    public static string EscapeQuotes(string str)
    {
        return str.Replace("'", "\\'").Replace("\"", "\\\"");
    }

    public static string ReverseString(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        char[] charArray = str.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray).Trim('\'');
    }
}
