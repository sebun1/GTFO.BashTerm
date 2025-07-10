using System.Text;
using System.Text.RegularExpressions;
using BashTerm.Exec;

namespace BashTerm.Utils;

public interface IFmtToStringable
{
	string FmtToString();
}

public static class Fmt
{
    public const int TerminalColWidth = 80;

    public static string Indent(VarCommand cmd) {
	    return Indent(cmd.FmtToString(), 2);
    }

    public static string Indent(string input, int indentSpaces = 4) {
	    return input.Replace("\n", "\n" + new string(' ', indentSpaces));
    }

    public static string Wrap(string input, int indentSpaces = 4, int maxCols = TerminalColWidth) {
	    return string.Join('\n', WrapList(input, indentSpaces, maxCols));
    }

    public static List<string> WrapList(string input, int indentSpaces = 4, int maxCols = TerminalColWidth) {
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

		    var wrappedLines = WrapLine(lineContent, maxCols, baseIndent);
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

	    return resultLines;
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
