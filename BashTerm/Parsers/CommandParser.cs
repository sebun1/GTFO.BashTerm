using System;
using System.Diagnostics;
using BashTerm.Exec;

namespace BashTerm.Parsers;

internal class MainParser {
	public static ParseResult Parse(string input) {
		var parser = new Parser(input);

		if (parser.PeekToken() is TokenEof)
			return new ParsedCommand(new EmptyCommand());

		try {
			var command = parser.ParseCommands();
			return new ParsedCommand(command);
		} catch (Exception e) {
			return new ParseError(e.ToString());
		}
	}
}

abstract record ParseResult;

record ParsedCommand(Command cmd) : ParseResult;

record ParseError(string cause) : ParseResult;

internal class Parser {
	private Lexer lexer;
	private Queue<Token> tokens;

	public Parser(string input) {
		this.lexer = new Lexer(input);
		this.tokens = new Queue<Token>();
		tokenizeInput();
	}

	private void tokenizeInput() {
		bool isFirstWord = true;
		while (true) {
			Token tok = lexer.Peek();
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
					throw new Exception("impossible");
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
		} else if (PeekToken() is TokenSemicolon) {
			NextToken();
			return new Sequence(cmd, ParseCommands());
		} else {
			return cmd;
		}
	}

	public Command ParseOneCommand() {
		Token tok = PeekToken();

		if (tok is TokenWord(string word)) {
			NextToken();

			return new Execve(word, ParseArgs());

			/*
			return word switch {
				"list" => new ListCommand(ParseArgs()),
				"query" => new QueryCommand(ParseArgs()),
				"ping" => new PingCommand(ParseArgs()),
				"reactor_startup" => new ReactorStartCommand(),
				"reactor_shutdown" => new ReactorStopCommand(),
				"reactor_verify" => new ReactorVerifyCommand(ParseArg()),
				"uplink_connect" => new UplinkStartCommand(ParseArg()),
				"uplink_verify" => new UplinkVerifyCommand(ParseArg()),
				"uplink_confirm" => new UplinkConfirmCommand(),
				"logs" => new LogsCommand(),
				"read" => new ReadCommand(ParseArg()),
				"start" => new StartCommand(ParseArg()),
				"info" => new InfoCommand(),
				"help" => new HelpCommand(),
				"exit" => new ExitCommand(),
				"cls" => new ClearCommand(),
				"raw" => new RawCommand(),
				_ => new SpecialCommand(string.Join(" ", new List<string>{word}.Concat(ParseArgs())))
				//_ => throw new Exception($"unrecognized command: {word}")
			};
			*/
		}

		throw new Exception($"command must start with a word, got: {tok}");
	}

	public string ParseArg() {
		Token tok = PeekToken();

		if (tok is TokenWord(string arg)) {
			NextToken();
			return arg;
		} else {
			throw new Exception($"expected argument, got {tok}");
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
