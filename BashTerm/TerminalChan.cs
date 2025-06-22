namespace BashTerm;

internal static class TerminalChan {
	private static bool _rawMode;
	private static int _infoCount;
	private static int _warnCount;
	private static int _errorCount;

	public static List<string> BSHLogs = new List<string>();
	public static bool RawMode {
		get { return _rawMode; }
	}

	public static int LogInfoCount {
		get { return _infoCount; }
	}

	public static int LogWarnCount {
		get { return _warnCount; }
	}

	public static int LogErrorCount {
		get { return _errorCount; }
	}

	public static void ToggleRawMode() {
		_rawMode = !_rawMode;
	}

	public static void LogInfo(string src, string msg) {
		_infoCount++;
		BSHLogs.Add($"{Clr.Info}[{src}] >> {msg}{Clr.End}");
	}

	public static void LogWarn(string src, string msg) {
		_warnCount++;
		BSHLogs.Add($"{Clr.Warning}[{src}] >> {msg}{Clr.End}");
	}

	public static void LogError(string src, string msg) {
		_errorCount++;
		BSHLogs.Add($"{Clr.Error}[{src}] >> {msg}{Clr.End}");
	}
}

public class BSHException : Exception {
	public BSHException(string message) : base(message) {}
}
