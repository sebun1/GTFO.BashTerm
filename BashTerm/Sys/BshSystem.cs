using UnityEngine;

namespace BashTerm.Sys;

internal class BshSystem : MonoBehaviour {
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
		BSHLogs.Add($"{Styles.Info}INFO[{src}] >> {msg}{Styles.CEnd}");
	}

	public static void LogWarn(string src, string msg) {
		_warnCount++;
		BSHLogs.Add($"{Styles.Warning}WARN[{src}] >> {msg}{Styles.CEnd}");
	}

	public static void LogError(string src, string msg) {
		_errorCount++;
		BSHLogs.Add($"{Styles.Error}ERRR[{src}] >> {msg}{Styles.CEnd}");
	}

	public void Update() {

	}
}

public class BSHException : Exception {
	public BSHException(string message) : base(message) {}
}
