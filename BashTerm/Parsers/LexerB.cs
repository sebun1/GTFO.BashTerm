using System.Text;
using BashTerm.Utils;

namespace BashTerm.Parsers;

public class LexerB {
	private int pos;
	private char[] input;
	private List<Token> tokens = new();
	private List<WordPart> wordPartCache = new();
	private StringBuilder wpb = new();
	private bool inDoubleQuote = false;
	private bool inSingleQuote = false;
	private bool wantVar = false;
	private bool escape = false;

	public LexerB(string input) {
		this.input = input.ToCharArray();
		tokenize();
	}

	private void tokenize() {
		while (true) {
			var (c, cc) = classify(pos++);

			if (cc == CharClass.Eof) {
				CtxState cs = getContextState();
				if (cs != CtxState.CLEAN) switch (cs) {
					case CtxState.ESC:
						throw new LexerException("Unexpected EOF: incomplete escape sequence");
					case CtxState.IDQ:
						throw new LexerException("Unexpected EOF: unclosed double quote");
					case CtxState.ISQ:
						throw new LexerException("Unexpected EOF: unclosed single quote");
					case CtxState.VAR:
						throw new LexerException("Unexpected EOF: variable expression expected");
				}
				flushWord();
				tokens.Add(new TokenEof());
				break;
			}

			if (cc == CharClass.White && !inDoubleQuote && !inSingleQuote) {
				flushWord();
				continue;
			}

			if (cc == CharClass.Semicolon && !inDoubleQuote && !inSingleQuote) {
				flushWord();
				tokens.Add(new TokenSemicolon());
				continue;
			}

			if (cc == CharClass.Pipe && !inDoubleQuote && !inSingleQuote) {
				flushWord();
				tokens.Add(new TokenPipe());
				continue;
			}

			if (cc == CharClass.DoubleQuote && !inSingleQuote) {
				inDoubleQuote = !inDoubleQuote;
				continue;
			}

			if (cc == CharClass.SingleQuote && !inDoubleQuote) {
				inSingleQuote = !inSingleQuote;
				continue;
			}

			if (cc == CharClass.Backslash && !inSingleQuote) {
				escape = true;
				continue;
			}

			if (cc == CharClass.Dollar && !inSingleQuote) {
				flushWpbToText();
				wantVar = true;
				parseVariableExpansion();
				continue;
			}

			wpb.Append(c);
		}
	}

	private void flushWpbToText() {
		if (wpb.Length > 0) {
			wordPartCache.Add(new WordText(wpb.ToString()));
			wpb.Clear();
		}
	}

	private void flushWord() {
		flushWpbToText();
		if (wordPartCache.Count > 0) {
			tokens.Add(new TokenWord(wordPartCache.ToArray()));
			wordPartCache.Clear();
		}
	}

	private void parseVariableExpansion() {
		var varName = new StringBuilder();

		if (pos < input.Length && input[pos] == '{') {
			pos++; // skip '{'
			while (pos < input.Length) {
				var (c, cc) = classify(pos++);
				if (cc == CharClass.CloseBrace) break;
				if (cc == CharClass.Eof) throw new LexerException("unclosed variable expansion");
				if (!isValidVarChar(c)) throw new LexerException($"bad substitution: char '{c}'");
				varName.Append(c);
			}
		} else {
			var (c, cc) = classify(pos++);
			if (!isValidVarChar(c)) throw new LexerException($"bad substitution: char '{c}'");
			varName.Append(c);
			if (!char.IsDigit(c)) {
				while (pos < input.Length) {
					var (next, _) = classify(pos);
					if (!isValidVarChar(next)) break;
					varName.Append(next);
					pos++;
				}
			}
		}

		wordPartCache.Add(new WordVar(varName.ToString()));
		wantVar = false;
	}

	private (char, CharClass) classify(int position) {
		if (position >= input.Length) return ((char)0, CharClass.Eof);

		char c = input[position];

		if (escape) {
			escape = false;
			return (c, CharClass.WordChar);
		}

		switch (c) {
			case '"': return (c, CharClass.DoubleQuote);
			case '\'': return (c, CharClass.SingleQuote);
			case '\\': return (c, CharClass.Backslash);
			case '|': return (c, CharClass.Pipe);
			case ';': return (c, CharClass.Semicolon);
			case '$': return (c, CharClass.Dollar);
			case '{': return (c, CharClass.OpenBrace);
			case '}': return (c, CharClass.CloseBrace);
		}

		if (char.IsWhiteSpace(c)) return (c, CharClass.White);
		return (c, CharClass.WordChar);
	}

	/*
	private (char, CharClass) classify(int position) {
		if (position >= input.Length) {
			return ((char)0, CharClass.Eof);
		}

		char c = input[position];

		if (escape) {
			escape = false;
			return (c, CharClass.WordChar);
		}

		if (Char.IsWhiteSpace(c) && !inDoubleQuote && !inSingleQuote) {
			return (c, CharClass.White);
		}

		switch (c) {
			case '"':
				if (!inSingleQuote) return (c, CharClass.DoubleQuote);
				break;
			case '\'':
				if (!inDoubleQuote) return (c, CharClass.SingleQuote);
				break;
			case '\\':
				if (!inSingleQuote) return (c, CharClass.Backslash);
				break;
			case '|':
				if (!inDoubleQuote && !inSingleQuote) return (c, CharClass.Pipe);
				break;
			case ';':
				if (!inDoubleQuote && !inSingleQuote) return (c, CharClass.Semicolon);
				break;
			case '$':
				if (!inSingleQuote) return (c, CharClass.Dollar);
				break;
			case '{':
				if (!inSingleQuote) return (c, CharClass.OpenBrace);
				break;
			case '}':
				if (!inSingleQuote) return (c, CharClass.CloseBrace);
				break;
		}

		return (c, CharClass.WordChar);
	}
	*/


	private bool isValidVarChar(char c) => char.IsLetterOrDigit(c) || c == '_';

	private enum CtxState {
		CLEAN,
		ESC,
		ISQ,
		IDQ,
		VAR,
	}

	private CtxState getContextState() {
		if (escape) return CtxState.ESC;
		if (inSingleQuote) return CtxState.ISQ;
		if (inDoubleQuote) return CtxState.IDQ;
		if (wantVar) return CtxState.VAR;
		return CtxState.CLEAN;
	}

	public List<Token> GetTokens() => tokens;

	private enum CharClass {
		White,
		WordChar,
		Pipe,
		Semicolon,
		DoubleQuote,
		SingleQuote,
		Dollar,
		OpenBrace,
		CloseBrace,
		Backslash,
		Eof
	}
}

public abstract record Token;

public record TokenWord(WordPart[] parts) : Token, IFmtToStringable {
	public override string ToString() {
		return $"\"{string.Join<WordPart>("", parts)}\"";
	}

	public string FmtToString() {
		if (parts.Length == 0) return "";
		if (parts.Length == 1) return parts[0].FmtToString();
		string formattedParts = string.Join("", parts.Select(p => p.FmtToString()));
		return $"\"{formattedParts}\"";
	}
};

public abstract record WordPart : IFmtToStringable {
	public abstract string FmtToString();
};

public record WordText(string text) : WordPart {
	public override string ToString() => $"{text}";

	public override string FmtToString() => $"{text}";
};

public record WordVar(string varName) : WordPart {
	public override string ToString() => $"${{{varName}}}";
	public override string FmtToString() => $"<#66D9EF>${varName}</color>";
};

public record TokenPipe() : Token {
	public override string ToString() => "Token[|]";
};

public record TokenSemicolon() : Token {
	public override string ToString() => "Token[;]";
};

public record TokenEof() : Token {
	public override string ToString() => "Token[EOF]";
};
