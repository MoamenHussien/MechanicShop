using System.Text;

public static class Utilities
{
    public static string CapitalizeFirstLetter(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        ReadOnlySpan<char> span = input.Trim();
        var result = new StringBuilder(span.Length);
        bool newWord = true;

        foreach (char c in span)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!newWord)
                {
                    result.Append(c);
                    newWord = true;
                }
            }
            else
            {
                result.Append(newWord ? char.ToUpper(c) : char.ToLower(c));
                newWord = false;
            }
        }

        return result.ToString();
    }
}