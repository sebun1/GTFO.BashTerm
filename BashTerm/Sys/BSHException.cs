namespace BashTerm.Sys;

public class BshException : Exception {
	public BshException(string message) : base(message) {}
}

public class BshSystemException : BshException {
	public BshSystemException(string message) : base($"[SystemError] >> {message}") {}
}
