using BashTerm.Sys;

namespace BashTerm.Parsers;

public class ParserException : BshException {
	public ParserException(string cause) : base($"[ParseError] >> {cause}") {}
}

public class UnexpectedTokenException : ParserException {
	public UnexpectedTokenException(string cause) : base(cause) {}
}

public class LexerException : ParserException {
	public LexerException(string cause) : base($"[LexerError] >> {cause}") {}
}


