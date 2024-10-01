using System.Reflection;
using System.Text;

namespace PiDarkPhotosUtilities;

public static class CommandLineTools
{
    public static IEnumerable<string> QuotePreservingParseForCommandLine(string? line)
    {
        //Based on https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes
        var delimiter = ' ';
        char[] textQualifier = ['"', '\''];
        char? currentInStringQualifier = null;

        if (line == null)
            yield break;

        var inString = false;

        var token = new StringBuilder();

        for (var i = 0; i < line.Length; i++)
        {
            var currentChar = line[i];

            var prevChar = i > 0 ? line[i - 1] : '\0';

            var nextChar = i + 1 < line.Length ? line[i + 1] : '\0';

            if (textQualifier.Contains(currentChar) && (prevChar == '\0' || prevChar == delimiter) && !inString)
            {
                currentInStringQualifier = currentChar;
                token.Append(currentChar);
                inString = true;
                continue;
            }

            if (currentChar == currentInStringQualifier && (nextChar == '\0' || nextChar == delimiter) && inString)
            {
                token.Append(currentChar);
                inString = false;
                continue;
            }

            if (currentChar == delimiter && !inString)
            {
                yield return token.ToString();
                token = token.Remove(0, token.Length);
                continue;
            }

            token = token.Append(currentChar);
        }

        yield return token.ToString();
    }

    public static void CleanStringProperties<T>(T obj)
    {
        var stringProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.CanWrite);

        foreach (var property in stringProperties)
            if (property.GetValue(obj) is string value)
            {
                while ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                       (value.StartsWith("'") && value.EndsWith("'"))) value = value.Substring(1, value.Length - 2);
                property.SetValue(obj, value);
            }
    }
}