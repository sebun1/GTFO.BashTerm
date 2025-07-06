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
	private LexerB lexer;
	private Queue<Token> tokens;

	public Parser(string input) {
		this.lexer = new LexerB(input);
		this.tokens = new Queue<Token>();
		bool firstWord = true;
		foreach (Token tk in this.lexer.GetTokens())
		{
			if (firstWord) {
				firstWord = false;
			}
		}
		if (ConfigMaster.DEBUG) {
			foreach (var token in this.tokens) {
				Logger.Debug(token.ToString());
			}
		}
	}

	public Token PeekToken() {
		return tokens.Count > 0 ? tokens.Peek() : new TokenEof();
	}

	public Token NextToken() {
		return tokens.Count > 0 ? tokens.Dequeue() : new TokenEof();
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
