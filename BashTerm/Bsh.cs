using LevelGeneration;

namespace BashTerm;

public class Bsh {
	public static bool Callable = false;
	public static LG_ComputerTerminal? Terminal;

	/// <summary>
	/// Renews the BSH validity with terminal instance
	/// </summary>
	/// <param name="term"></param>
	/// <returns></returns>
	internal static bool Renew(LG_ComputerTerminal term) {
		if (term != null) {
			Terminal = term;
			Callable = true;
			return true;
		}

		Terminal = null;
		Callable = false;
		return false;
	}

	/// <summary>
	/// Expires the BSH validity, makes not Callable
	/// </summary>
	internal static void Expire() {
		Terminal = null;
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
