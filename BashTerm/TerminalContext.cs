using LevelGeneration;
using UnityEngine.Playables;

namespace BashTerm;

internal static class TerminalContext {
	public static LG_TERM_PlayerInteracting? termInteractionInstance;
	public static bool _rawMode = false;
	public static bool _inTerminal = false;

	public static bool inTerminal {
		get { return _inTerminal; }
	}

	public static bool rawMode {
		get { return _rawMode; }
	}

	public static bool enterTerminal() {
		Logger.Debug("enterTerminal");
		bool prev = _inTerminal;
		_inTerminal = true;
		return prev ^ true;
	}

	public static bool exitTerminal() {
		Logger.Debug("exitTerminal");
		bool prev = _inTerminal;
		_inTerminal = false;
		return prev ^ false;
	}

	public static bool addTermOutput(string str) {
		if (termInteractionInstance == null || termInteractionInstance.m_terminal == null) return false;

		termInteractionInstance.m_terminal.AddLine(str);
		return true;
	}

	public static void toggleRawMode() {
		_rawMode = !_rawMode;
	}
}
