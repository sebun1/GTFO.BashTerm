using LevelGeneration;
using UnityEngine.Playables;

namespace BashTerm;

internal static class TerminalContext {
	private static LG_ComputerTerminal? _terminalInstance;

	public static LG_ComputerTerminal? TerminalInstance {
		get => _terminalInstance;
	}

	public static bool _rawMode = false;
	public static bool _inTerminal = false;

	public static bool InTerminal {
		get { return _inTerminal; }
	}

	public static bool rawMode {
		get { return _rawMode; }
	}

	public static bool EnterTerminal(LG_ComputerTerminal termInstance) {
		_terminalInstance = termInstance;

		Logger.Debug("EnterTerminal");
		if (_inTerminal) Logger.Warn("[TerminalContext] EnterTerminal called when _inTerminal is already true");
		bool prev = _inTerminal;
		_inTerminal = true;
		return prev ^ true;
	}

	public static bool ExitTerminal() {
		Logger.Debug("ExitTerminal");
		if (!_inTerminal) Logger.Warn("[TerminalContext] ExitTerminal called when _inTerminal is already false");
		bool prev = _inTerminal;
		_inTerminal = false;
		return prev ^ false;
	}

	public static void EvaluateInput(string input) {
		if (_terminalInstance == null) return;
		_terminalInstance.m_command.EvaluateInput(input);
	}

	public static void WantToSendCommand(TERM_Command cmd, string input, string arg1, string arg2) {
		if (_terminalInstance == null) {
			Logger.Error("[TerminalContext] _terminalInstance is null on WantToSendCommand()");
			return;
		}
		LG_ComputerTerminalManager.WantToSendTerminalCommand(_terminalInstance.SyncID, cmd, input, arg1, arg2);
	}
	public static bool AddLine(string str) { // Injects line into display, bypassing GTFO routines
		if (_terminalInstance == null) {
			Logger.Error("[TerminalContext] _terminalInstance is null on AddLine()");
			return false;
		}
		_terminalInstance.AddLine(str);
		return true;
	}

	public static bool AddOutput(string str, bool spacing = true) { // This follows GTFO routines
		if (_terminalInstance == null) {
			Logger.Error("[TerminalContext] _terminalInstance is null on AddOutput()");
			return false;
		}
		_terminalInstance.m_command.AddOutput(str, spacing);
		return true;
	}

	public static void ToggleRawMode() {
		_rawMode = !_rawMode;
	}
}
