namespace BashTerm.Parsers;

internal class Lexer {
	enum CharClass {
		White, // whitespace
		WordChar, // alphabetical character & word
		Pipe, // "|"
		Eof // end-of-file / end-of-line
	}

	enum LexState {
		Start,
		SeenWord, // Saw a word token
		DoneWord,
		DonePipe, // Saw a "|"
		DoneEof, // Saw end-of-file/line
	}

	readonly Dictionary<(LexState, CharClass), LexState> transitions = new Dictionary<(LexState, CharClass), LexState> {
		{ (LexState.Start, CharClass.White), LexState.Start },
		{ (LexState.Start, CharClass.WordChar), LexState.SeenWord },
		{ (LexState.SeenWord, CharClass.WordChar), LexState.SeenWord },
		{ (LexState.SeenWord, CharClass.White), LexState.DoneWord },
		{ (LexState.SeenWord, CharClass.Pipe), LexState.DoneWord },
		{ (LexState.SeenWord, CharClass.Eof), LexState.DoneWord },
		{ (LexState.Start, CharClass.Pipe), LexState.DonePipe },
		{ (LexState.Start, CharClass.Eof), LexState.DoneEof }
	};

	private int position;
	private string input;
	private Token token;

	public Lexer(string input) {
		this.position = 0;
		this.input = input;
		this.token = Next();
	}

	// Lex a token.

	private Token Next() {
		int pos0 = position;

		LexState state = LexState.Start;

		while (true) {
			var c = classify(input, position++);
			state = transitions[(state, c)];
			if (isFinished(state)) {
				return convertToken(state, input, pos0, position - 1);
			}

			if (state == LexState.Start) {
				pos0 = position;
			}
		}
	}

	public void Consume() {
		this.token = Next();
	}

	public Token Peek() {
		return this.token;
	}

	private static bool isFinished(LexState state) =>
		state switch {
			LexState.DoneWord => true,
			LexState.DonePipe => true,
			LexState.DoneEof => true,
			_ => false
		};

	private static bool isWordChar(char c) =>
		Char.IsLetterOrDigit(c) || "_-./:=+@%~*()[]".Contains(c); //{} is for variable

	private static Token convertToken(LexState state, string input, int start, int end) =>
		state switch {
			LexState.DoneWord => new TokenWord(input.Substring(start, end - start)),
			LexState.DonePipe => new TokenPipe(),
			LexState.DoneEof => new TokenEof(),
			_ => throw new Exception("impossible")
		};

	private CharClass classify(string input, int position) {
		if (position >= input.Length) {
			return CharClass.Eof;
		}

		var c = input[position];

		if (Char.IsWhiteSpace(c)) {
			return CharClass.White;
		}

		if (isWordChar(c)) {
			return CharClass.WordChar;
		} else if (c == '|') {
			return CharClass.Pipe;
		} else {
			throw new Exception($"couldn't classify character: {c}");
		}
	}
}

abstract record Token;

record TokenWord(string word) : Token; // "word"

record TokenPipe() : Token; // "|"

record TokenSemicolon() : Token; // ";" // TODO: Add semicolon and Sequence(l, r)

record TokenEof() : Token;
