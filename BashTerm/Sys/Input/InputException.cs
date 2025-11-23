namespace Bsh.Sys.Input;

public class InputException : BshSystemException {
	public InputException(string message) : base($"[InputException] >> {message}") {
	}
}
