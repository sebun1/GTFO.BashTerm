using System;
using System.Diagnostics;
using BashTerm.Exec;

namespace BashTerm.Parsers;

internal class MainParser {
	public static ParsedCommand Parse(string input) {
		var parser = new Parser(input);

		if (parser.PeekToken() is TokenEof)
			return new ParsedCommand(new Execve("", new List<string>()));

		var command = parser.ParseCommands();
		return new ParsedCommand(command);
	}
}

record ParsedCommand(Command cmd);

internal class Parser {
	private Lexer lexer;
	private Queue<Token> tokens;

	public Parser(string input) {
		this.lexer = new Lexer(input);
		this.tokens = new Queue<Token>();
		tokenizeInput();
		if (ConfigMaster.DEBUG) {
			foreach (var token in this.tokens) {
				Logger.Debug(token.ToString());
			}
		}
	}

	private void tokenizeInput() {
		bool isFirstWord = true;
		while (true) {
			Token tok = lexer.Peek();
			Logger.Debug($"tokenizeInput: {tok}");
			switch (tok) {
				case TokenEof:
					return;

				case TokenWord(string word):
					if (isFirstWord) {
						string expanded = ParseUtil.ExpandCmd(word);
						Lexer expLexer = new Lexer(expanded);
						while (expLexer.Peek() is not TokenEof) {
							tokens.Enqueue(expLexer.Peek());
							expLexer.Consume();
						}
						isFirstWord = false;
					} else {
						tokens.Enqueue(new TokenWord(word));
					}
					lexer.Consume();
					break;

				case TokenPipe:
					tokens.Enqueue(new TokenPipe());
					lexer.Consume();
					isFirstWord = true;
					break;

				case TokenSemicolon:
					tokens.Enqueue(new TokenSemicolon());
					lexer.Consume();
					isFirstWord = true;
					break;

				default:
					throw new ParserException("impossible");
			}
		}
	}

	public Token PeekToken() {
		return tokens.Count > 0 ? tokens.Peek() : new TokenEof();
	}

	public Token NextToken() {
		return tokens.Count > 0 ? tokens.Dequeue() : new TokenEof();
	}

	public Command ParseCommands() {
		var cmd = ParseOneCommand();

		if (PeekToken() is TokenPipe) {
			NextToken();
			return new Pipe(cmd, ParseCommands());
		}
		if (PeekToken() is TokenSemicolon) {
			NextToken();
			return new Sequence(cmd, ParseCommands());
		}
		return cmd;
	}

	public Command ParseOneCommand() {
		Token tok = PeekToken();

		if (tok is TokenWord(string word)) {
			NextToken();
			return new Execve(word, ParseArgs());
		}

		throw new ParserException($"command must start with a word, got: {tok}");
	}

	public string ParseArg() {
		Token tok = PeekToken();

		if (tok is TokenWord(string arg)) {
			NextToken();
			return arg;
		} else {
			throw new ParserException($"expected argument, got {tok}");
		}
	}

	public List<string> ParseArgs() {
		List<string> args = new List<string>();
		while (PeekToken() is TokenWord(string arg)) {
			NextToken();
			args.Add(arg);
		}

		return args;
	}
}
