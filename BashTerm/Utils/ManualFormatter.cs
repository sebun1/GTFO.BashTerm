using System.Text;
using System.Text.RegularExpressions;

namespace BashTerm;

public static class ManualFormatter
{
    public const int TERMINAL_COL_WIDTH = 80;

    public static string GetFormattedManual(string input, int indentSpaces = 4)
    {
        var resultLines = new List<string>();
        foreach (var line in input.Split('\n'))
        {
            // Count leading tabs
            int tabCount = 0;
            foreach (char c in line)
            {
                if (c == '\t') tabCount++;
                else break;
            }

            string lineContent = line.TrimStart('\t');
            string baseIndent = new string(' ', tabCount * indentSpaces);

            var wrappedLines = WrapLine(lineContent, TERMINAL_COL_WIDTH, baseIndent);
            if (wrappedLines.Count > 0)
            {
                resultLines.Add(baseIndent + wrappedLines[0]); // First line
                for (int i = 1; i < wrappedLines.Count; i++)
                    resultLines.Add(baseIndent + wrappedLines[i]); // Wrapped lines
            }
            else
            {
                resultLines.Add(baseIndent); // Blank line
            }
        }

        return string.Join("\n", resultLines);
    }

    private static List<string> WrapLine(string input, int maxWidth, string baseIndent)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        int currentWidth = 0;

        foreach (var token in Tokenize(input))
        {
            int tokenLen = token.Length;

            // Wrap if too long
            if (currentWidth + tokenLen > maxWidth - baseIndent.Length)
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    currentWidth = 0;
                }

                // If token itself is too big, break it char by char
                if (tokenLen > maxWidth - baseIndent.Length)
                {
                    foreach (char c in token)
                    {
                        if (currentWidth + 1 > maxWidth - baseIndent.Length)
                        {
                            result.Add(current.ToString());
                            current.Clear();
                            currentWidth = 0;
                        }
                        current.Append(c);
                        currentWidth++;
                    }
                }
                else
                {
                    current.Append(token);
                    currentWidth += tokenLen;
                }
            }
            else
            {
                current.Append(token);
                currentWidth += tokenLen;
            }
        }

        if (current.Length > 0)
            result.Add(current.ToString());

        return result;
    }

    private static IEnumerable<string> Tokenize(string input)
    {
        var pattern = @"(\p{IsCJKUnifiedIdeographs}|\p{IsHiragana}|\p{IsKatakana}|[^\p{IsCJKUnifiedIdeographs}\s]+|\s+)";
        foreach (Match m in Regex.Matches(input, pattern))
            yield return m.Value;
    }
}
