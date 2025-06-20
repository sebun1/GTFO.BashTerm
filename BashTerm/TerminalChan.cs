namespace BashTerm;

internal static class TerminalChan {
	public static bool _rawMode;

	public static bool RawMode {
		get { return _rawMode; }
	}

	public static void ToggleRawMode() {
		_rawMode = !_rawMode;
	}
}

public class BSHException : Exception {
	public BSHException(string message) : base(message) {}
}
