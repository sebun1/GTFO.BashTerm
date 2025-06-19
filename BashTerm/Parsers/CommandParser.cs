using System;
using System.Diagnostics;

namespace BashTerm.Parsers;

internal class MainParser {
	// Main entry point of the command parser.

	public static ParseResult Parse(string input) {
		var parser = new Parser(input);

		if (parser.PeekToken() is TokenEof)
			return new ParsedCommand(new EmptyCommand());

		try {
			var command = parser.ParseCommands();
			return new ParsedCommand(command);
		} catch () {
		} catch (Exception e) {
			return new ParseError(e.ToString());
		}
	}
}

abstract record ParseResult;

record ParsedCommand(Command cmd) : ParseResult;

record ParseError(string cause) : ParseResult;

internal class ParseException : Exception {
	public int Position { get; }

	public ParseException(string cause, int position) : base(cause) {
		this.Position = position;
	}

	public override string ToString() => $"[ParseError] {Message}";
}

internal class TooManyArgumentsException : ParseException {
	public string CommandName { get; }

	public TooManyArgumentsException(string commandName, int position, int got, int expected)
		: base($"Command '{commandName}' received too many arguments, want {expected}, got {got} [col {position}]", position) {
		CommandName = commandName;
	}
}

internal class MissingArgumentException : ParseException {
	public string CommandName { get; }

	public MissingArgumentException(string commandName, int position, int got, int expected)
		: base($"Command '{commandName}' is missing arguments, want {expected}, got {got} [col {position}]", position) {
		CommandName = commandName;
	}
}

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
		} else {
			return cmd;
		}
	}

	public Command ParseOneCommand() {
		Token tok = PeekToken();

		if (tok is TokenWord(string word)) {
			NextToken();

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

	public List<string> ParseArgs(int expected = 0) {
		List<string> args = new List<string>();
		while (PeekToken() is TokenWord(string arg)) {
			NextToken();
			args.Add(arg);
		}

		return args;
	}
}
