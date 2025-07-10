namespace BashTerm.Parsers;

public class ParserException : BSHException {

	public ParserException(string cause) : base(cause) {}

	public override string ToString() => $"[ParseError] >> {Message}";
}

public class UnexpectedTokenException : ParserException {
	public UnexpectedTokenException(string cause) : base(cause) {}
}

public class LexerException : ParserException {
	public LexerException(string cause) : base(cause) {}

	public override string ToString() => $"[LexerError] >> {Message}";
}


