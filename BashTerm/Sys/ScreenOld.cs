using LevelGeneration;
using TMPro;
using UnityEngine;

namespace BashTerm.Sys;

public class ScreenOld {
	public enum ScreenType {
		Shell,
		Discrete,
	}

	public readonly int Cols = 150;
	public readonly int Rows = 52;
	public readonly int ScreenID;
	public readonly List<string> History;

	private string InputLine;
	private int LastPromptRow;
	private int CursorPosition;

	public readonly ScreenType Type;

	private PipeStream _stream;

	private string Buffer_;

	public string Buffer {
		get { return Buffer_; }
	}

	private int Position;

	internal ScreenOld(int screenID, ScreenType type, PipeStream stream) {
		ScreenID = screenID;
		Type = type;
		_stream = stream;
		History = new();
		Buffer_ = "";
		Position = 0;
		_stream.SetReadingScreen(this);
	}

	public bool SetBuffer(string text) {
		if (Type == ScreenType.Discrete) {
			Buffer_ = text;
			return true;
		}

		return false;
	}

	internal void Seek(int step) {
		Position = Math.Clamp(Position + step, 0, History.Count - 1);
		int start = Math.Max(0, Position - Rows + 1);
		int left = Rows - (Position - start + 1);
		int end = Math.Min(History.Count - 1, Position + left);
		Buffer_ = string.Join('\n', History.ToArray(), start, end - start + 1);
	}


	internal void Insert(char c) {
		InputLine = InputLine.Insert(CursorPosition, c.ToString());
		CursorPosition++;
		ValidateStates();
	}

	internal void Insert(string str) {
		InputLine = InputLine.Insert(CursorPosition, str);
		CursorPosition += str.Length;
		ValidateStates();
	}

	internal enum Action {
		Move,
		Delete,
	}

	internal enum Motion {
		WordBack,
		WordForward,
		CharBack,
		CharForward,
		LineBack,
		LineForward,
	}

	internal bool Do(Action act, Motion motion) {
		if (Type != ScreenType.Shell) {
			Debug.LogWarning("Do() is only applicable for Shell screens.");
			return false;
		}

		int delta = GetCursorDelta(motion);
		switch (act) {
			case Action.Move:
				CursorPosition += delta;
				break;
			case Action.Delete:
				InputLine = InputLine.Remove(delta < 0 ? CursorPosition + delta : CursorPosition, Mathf.Abs(delta));
				if (delta < 0)
					CursorPosition += delta;
				break;
		}

		ValidateStates();
		return true;
	}

	private int GetCursorDelta(Motion m) {
		return m switch {
			Motion.CharBack => CursorPosition == 0 ? 0 : -1,
			Motion.CharForward => CursorPosition == InputLine.Length ? 0 : 1,
			Motion.WordBack => GetWordBackDeltaSimple(),
			Motion.WordForward => GetWordForwardDeltaSimple(),
			Motion.LineBack => -CursorPosition,
			Motion.LineForward => InputLine.Length - CursorPosition,
			_ => 0
		};
	}

	private int GetWordBackDeltaSimple() {
		if (CursorPosition == 0) return 0;
		int lastSpaceIndex = InputLine.Substring(0, CursorPosition).LastIndexOf(' ');
		if (lastSpaceIndex == -1)
			return -CursorPosition;
		return -(CursorPosition - lastSpaceIndex);
	}

	private int GetWordForwardDeltaSimple() {
		if (CursorPosition >= InputLine.Length) return 0;
		int firstSpaceIndex = InputLine.IndexOf(' ', CursorPosition + 1);
		if (firstSpaceIndex == -1)
			return InputLine.Length - CursorPosition;
		return firstSpaceIndex - CursorPosition;
	}

	private void ValidateStates() {
		CursorPosition = Mathf.Clamp(CursorPosition, 0, InputLine.Length);
	}

	/// <summary>
	/// Clears the output of the current process
	/// </summary>
	public void ClearOutput() {
		if (History.Count <= LastPromptRow + 1) return;
		History.RemoveRange(LastPromptRow, History.Count - LastPromptRow);
	}

	/// <summary>
	/// Clear everything in the shell history, internal use only
	/// </summary>
	internal void ClearAll() {
		throw new NotImplementedException();
	}

	public void ClearInput() {
		InputLine = "";
		CursorPosition = 0;
	}
}
