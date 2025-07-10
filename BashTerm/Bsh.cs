using BashTerm.Utils;
using Il2CppSystem.Text.RegularExpressions;
using LevelGeneration;

namespace BashTerm;

public class Bsh {
	public static bool Callable = false;

	public static LG_ComputerTerminal? Terminal {
		get {
			Logger.Debug($"Terminal is using SyncID={_terminal?.SyncID}");
			return _terminal;
		}
	}

	public static LG_ComputerTerminal? _terminal;
	private static bool _isReceivingBSHSyncIO = false;
	private const int InputLineMaxCol = 50;

	/// <summary>
	/// Renews the BSH validity with terminal instance
	/// </summary>
	/// <param name="term"></param>
	/// <returns></returns>
	internal static bool Renew(LG_ComputerTerminal term) {
		if (term != null) {
			_terminal = term;
			Callable = true;
			return true;
		}

		_terminal = null;
		Callable = false;
		return false;
	}

	/// <summary>
	/// Expires the BSH validity, makes not Callable
	/// </summary>
	internal static void Expire() {
		_terminal = null;
		Callable = false;
	}

	public static bool Println(string msg) {
		if (!Callable) return false;

		return true;
	}

	public static bool Print(string msg) {
		if (!Callable) return false;

		return true;
	}

	public static bool PrintScreen(List<string> screen) {
		if (!Callable) return false;

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

	public static bool IsSyncIOEnd(string line) {
		var pattern = @"^<7\.1@bsh>";
		return Regex.IsMatch(line, pattern);
	}

	public static bool SyncPrint(string msg) {
		if (!Callable) return false;
		var lines = Fmt.WrapList(msg, maxCols: InputLineMaxCol);
		string bshSyncOutputStart = $"<bsh@7.1> {Styles.Info}[ BSH Synced IO >> ]{Styles.CEnd}";
		string bshSyncOutputEnd = $"<7.1@bsh> {Styles.Info}[ BSH Synced IO << ]{Styles.CEnd}";


		LG_ComputerTerminalManager.WantToSendTerminalCommand(_terminal!.SyncID,
			TERM_Command.EmptyLine, bshSyncOutputStart, "", "");
		foreach (var line in lines) {
			LG_ComputerTerminalManager.WantToSendTerminalCommand(_terminal!.SyncID,
				TERM_Command.EmptyLine, line, "", "");
		}
		LG_ComputerTerminalManager.WantToSendTerminalCommand(_terminal!.SyncID,
			TERM_Command.EmptyLine, bshSyncOutputEnd, "", "");
		return true;
	}

	public static bool ReadLine(out string line) {
		if (!Callable) {
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
