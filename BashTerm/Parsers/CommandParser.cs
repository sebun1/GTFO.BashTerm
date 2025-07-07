using System;
using System.Diagnostics;
using BashTerm.Exec;
using BashTerm.Utils;
using Il2CppSystem.Data;

namespace BashTerm.Parsers;

internal class MainParser {
	public static VarCommand Parse(string input) {
		var parser = new Parser(input);

		if (parser.PeekToken() is TokenEof)
			return new VarExecve(new TokenWord(new WordPart[] { new WordText("") }), new());
		// TODO: ^ make cleaner way of declaring empty Execve

		var command = parser.ParseCommands();
		return command;
	}
}

internal class Parser {
	private Queue<Token> _tokens;

	public Parser(string input) {
		LexerB lexer = new LexerB(input);
		_tokens = new();
		bool firstWord = true;
		var tokens = lexer.GetTokens();
		foreach (Token tk in tokens) {
			switch (tk) {
				case TokenWord word:
					if (firstWord && word.parts.Length == 1 && word.parts[0] is WordText wt &&
					    ParseUtil.TryExpandAlias(wt.text, out var expansion)) {
						var expLexer = new LexerB(expansion);
						var expTokens = expLexer.GetTokens();
						foreach (Token expTk in expTokens) {
							if (expTk is TokenEof) break;
							_tokens.Enqueue(expTk);
						}
					} else {
						_tokens.Enqueue(tk);
					}
					firstWord = false;
					break;

				case TokenPipe:
				case TokenSemicolon:
				case TokenEof:
					firstWord = true;
					_tokens.Enqueue(tk);
					break;

				default:
					throw new UnexpectedTokenException($"got unexpected token from lexer: {tk.GetType().FullName}");
			}
		}

		if (ConfigMaster.DEBUG) {
			foreach (var token in this._tokens) {
				Logger.Debug(token.ToString());
			}
		}
	}

	public Token PeekToken() {
		return _tokens.Count > 0 ? _tokens.Peek() : new TokenEof();
	}

	public Token NextToken() {
		return _tokens.Count > 0 ? _tokens.Dequeue() : new TokenEof();
	}

	public VarCommand ParseCommands() {
		var cmd = ParseOneCommand();

		if (PeekToken() is TokenPipe) {
			NextToken();
			return new VarPipe(cmd, ParseCommands());
		}

		if (PeekToken() is TokenSemicolon) {
			NextToken();
			return new VarSequence(cmd, ParseCommands());
		}

		return cmd;
	}

	public VarCommand ParseOneCommand() {
		Token tok = PeekToken();

		if (tok is TokenWord word) {
			NextToken();
			return new VarExecve(word, ParseArgs());
		}

		throw new ParserException($"command must start with a word, got: {tok}");
	}

	public List<TokenWord> ParseArgs() {
		List<TokenWord> args = new();
		while (PeekToken() is TokenWord word) {
			NextToken();
			args.Add(word);
		}

		return args;
	}
}
