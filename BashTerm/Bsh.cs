using BashTerm.Utils;
using Il2CppSystem.Text.RegularExpressions;
using LevelGeneration;

namespace BashTerm;

public class Bsh {
	public static bool HasTerminal = false;

	internal static List<string> BSHLogs = new List<string>();

	public static int LogInfoCount { get { return _infoCount; } }
	public static int LogWarnCount { get { return _warnCount; } }
	public static int LogErrorCount { get { return _errorCount; } }

	private static int _infoCount;
	private static int _warnCount;
	private static int _errorCount;

	public static LG_ComputerTerminal? CurrentTerminal {
		get {
			Logger.Debug($"Terminal is using SyncID={_currentTerminal?.SyncID}");
			return _currentTerminal;
		}
	}

	public static LG_ComputerTerminal? _currentTerminal;
	private static bool _isReceivingBSHSyncIO = false;
	private const int InputLineMaxCol = 50;

	/// <summary>
	/// Renews the BSH validity with terminal instance
	/// </summary>
	/// <param name="term"></param>
	/// <returns></returns>
	internal static bool Renew(LG_ComputerTerminal term) {
		if (term != null) {
			_currentTerminal = term;
			HasTerminal = true;
			return true;
		}

		_currentTerminal = null;
		HasTerminal = false;
		return false;
	}

	/// <summary>
	/// Expires the BSH validity, makes not Callable
	/// </summary>
	internal static void Expire() {
		_currentTerminal = null;
		HasTerminal = false;
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


	public static bool PrintScreen(List<string> screen) {
		if (!HasTerminal) return false;

		//StringTools.ConvertScreenBufferToCharBuffer();
		return true;
	}

	/// <summary>
	/// Gets and detects if the line is part of SyncedIO. Updates internal record.
	/// </summary>
	/// <param name="line">input line sent/received</param>
	/// <returns>true if line is part of SyncedIO, otherwise false</returns>
	public static bool DetectSyncedIO(string line) {
		var startPat = @"^<bsh@7\.1>";
		var endPat = @"^<7\.1@bsh>";

		if (!_isReceivingBSHSyncIO) {
			_isReceivingBSHSyncIO = Regex.IsMatch(line, startPat);
			return _isReceivingBSHSyncIO;
		} else {
			_isReceivingBSHSyncIO = !Regex.IsMatch(line, endPat);
			return true;
		}
	}

	public static bool SyncPrint(string msg) {
		if (!HasTerminal) return false;
		var lines = Fmt.WrapList(msg, maxCols: InputLineMaxCol);
		string bshSyncOutputStart = $"<bsh@7.1> {Styles.Info}[ BSH Synced IO >> ]{Styles.CEnd}";
		string bshSyncOutputEnd = $"<7.1@bsh> {Styles.Info}[ BSH Synced IO << ]{Styles.CEnd}";


		LG_ComputerTerminalManager.WantToSendTerminalCommand(_currentTerminal!.SyncID,
			TERM_Command.EmptyLine, bshSyncOutputStart, "", "");
		foreach (var line in lines) {
			LG_ComputerTerminalManager.WantToSendTerminalCommand(_currentTerminal!.SyncID,
				TERM_Command.EmptyLine, line, "", "");
		}
		LG_ComputerTerminalManager.WantToSendTerminalCommand(_currentTerminal!.SyncID,
			TERM_Command.EmptyLine, bshSyncOutputEnd, "", "");
		return true;
	}

	public static bool ReadLine(out string line) {
		if (!HasTerminal) {
			line = "";
			return false;
		}

		line = "";
		return true;
	}

	public static bool GetButtonDown(InputAction act, eFocusState state) {
		return InputMapper.GetButtonDownKeyMouseGamepad(act, state);
	}

	public static bool GetButtonUp(InputAction act, eFocusState state) {
		return InputMapper.GetButtonUpKeyMouseGamepad(act, state);
	}
}
