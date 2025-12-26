using Bsh.Sys.Input;
using UnityEngine;

namespace Bsh.Sys.Sh;

public class LineEditor {
	public Action<LineEditor, string>? OnLineFeed;

	public string Line { get; private set; } = "";
	private int _cursorPos;

	/// <summary>
	/// Applies the keystroke to the current le state.
	/// </summary>
	/// <param name="key">keystroke to apply</param>
	/// <returns>true if the keystroke is recognized</returns>
	public bool Do(KeyStroke key) {
		if (key.Mods != KeyModifier.None && (key.Ctrl || key.Alt)) {
			return DoModifierKeystroke(key);
		}

		if (key.IsChar()) {
			InsertChar(key.Char);
			return true;
		}

		return false;
	}

	private bool DoModifierKeystroke(KeyStroke key) {
		if (key.Mods == KeyModifier.None) return false;

		// common emacs/zle bindings
		if (key is { Key: KeyCode.A, Ctrl: true, Shift: false, Alt: false }) {
			_cursorPos = 0;
			return true;
		}

		if (key is { Key: KeyCode.E, Ctrl: true, Shift: false, Alt: false }) {
			_cursorPos = Line.Length;
			return true;
		}

		if (key is { Key: KeyCode.D, Ctrl: true, Shift: false, Alt: false }) {
			// delete char at cursor
			if (_cursorPos < Line.Length) {
				Line = Line.Remove(_cursorPos, 1);
			}

			return true;
		}

		// Ctrl+Backspace: delete previous word (word backward)
		if (key is { Key: KeyCode.Backspace, Ctrl: true }) {
			int newPos = GetWordBackIndex(_cursorPos);
			int len = _cursorPos - newPos;
			if (len > 0) {
				Line = Line.Remove(newPos, len);
				_cursorPos = newPos;
			}
			return true;
		}

		// Ctrl+W: delete word backward (common shell binding)
		if (key is { Key: KeyCode.W, Ctrl: true, Alt: false }) {
			int newPos = GetWordBackIndex(_cursorPos);
			int len = _cursorPos - newPos;
			if (len > 0) {
				Line = Line.Remove(newPos, len);
				_cursorPos = newPos;
			}
			return true;
		}

		// Ctrl+K: kill to end of line
		if (key is { Key: KeyCode.K, Ctrl: true, Alt: false }) {
			if (_cursorPos < Line.Length) {
				Line = Line.Remove(_cursorPos, Line.Length - _cursorPos);
			}
			return true;
		}

		// Ctrl+U: kill to beginning of line
		if (key is { Key: KeyCode.U, Ctrl: true, Alt: false }) {
			if (_cursorPos > 0) {
				Line = Line.Remove(0, _cursorPos);
				_cursorPos = 0;
			}
			return true;
		}

		// Ctrl+Left / Alt+B: move word back
		if ((key is { Key: KeyCode.LeftArrow, Ctrl: true }) || (key is { Key: KeyCode.B, Alt: true })) {
			_cursorPos = GetWordBackIndex(_cursorPos);
			return true;
		}

		// Ctrl+Right / Alt+F: move word forward
		if ((key is { Key: KeyCode.RightArrow, Ctrl: true }) || (key is { Key: KeyCode.F, Alt: true })) {
			_cursorPos = GetWordForwardIndex(_cursorPos);
			return true;
		}

		return false;
	}

	private void InsertChar(char c) {
		if (_cursorPos == Line.Length) {
			Line += c;
		} else {
			Line = Line.Insert(_cursorPos, c.ToString());
		}

		_cursorPos++;
	}

	public void Clear() {
		Line = string.Empty;
		_cursorPos = 0;
	}

	/// <summary>
	/// Get index of start of previous word from position 'pos'.
	/// Words are delimited by spaces (simple heuristic) and punctuation is considered part of a word.
	/// </summary>
	private int GetWordBackIndex(int pos) {
		if (string.IsNullOrEmpty(Line) || pos <= 0) return 0;

		int i = Math.Clamp(pos - 1, 0, Line.Length - 1);
		// skip any spaces immediately before pos
		while (i >= 0 && Line[i] == ' ') i--;
		// then find previous space
		while (i >= 0 && Line[i] != ' ') i--;
		return i + 1;
	}

	/// <summary>
	/// Get index of start of next word from position 'pos'.
	/// </summary>
	private int GetWordForwardIndex(int pos) {
		if (string.IsNullOrEmpty(Line) || pos >= Line.Length) return Line.Length;

		int i = pos;
		// skip current non-space characters
		while (i < Line.Length && Line[i] != ' ') i++;
		// skip spaces to the start of next word
		while (i < Line.Length && Line[i] == ' ') i++;
		return i;
	}
}
