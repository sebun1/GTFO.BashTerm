namespace BashTerm.Parsers;

public class ParserException : BSHException {

	public ParserException(string cause) : base(cause) {}

	public override string ToString() => $"[ParseError] >> {Message}";
}

public class LexerException : ParserException {
	public LexerException(string cause) : base($"[LexerError] >> {cause}") {}
}


