using LevelGeneration;
using TMPro;
using UnityEngine;

namespace BashTerm.Sys;

public abstract class Screen {
	public readonly int Cols = 150;
	public readonly int Rows = 52;
	public readonly int ScreenID;
	public readonly List<string> History;
	protected Queue<string> OutputQueue;

	public string Buffer {
		get { return Buffer_; }
	}
	protected string Buffer_;

	protected int Position;
	protected int CursorRow = 0;
	protected int CursorCol = 0;
	protected bool CursorVisible = true;
	protected bool CursorBlinking = false;

	public enum CursorStyle {
		Block,
		Underline,
	}

	protected CursorStyle cursorStyle = CursorStyle.Block;

	public Screen(int screenID) {
		ScreenID = screenID;
		History = new List<string>();
		OutputQueue = new Queue<string>();
		Buffer_ = "";
		Position = 0;
	}

	public void Print(string txt) {
		OutputQueue.Enqueue(txt);
	}

	public void Println(string txt) {
		Print(txt + '\n');
	}

	public void Println(List<string> lines) {
		foreach (var line in lines)
			OutputQueue.Enqueue(line + '\n');
	}

	public virtual bool SetBuffer(string text) {

	}

	internal void Seek(int step) {
		Position = Math.Clamp(Position + step, 0, History.Count - 1);
		int start = Math.Max(0, Position - Rows + 1);
		int left = Rows - (Position - start + 1);
		int end = Math.Min(History.Count - 1, Position + left);
		Buffer_ = string.Join('\n', History.ToArray(), start, end - start + 1);
	}

}

public class ShellScreen : Screen {
	protected string InputLine;
	protected int LastPromptRow;
	protected int CursorPosition;
	private readonly BshPM pm;
	private readonly BshIO io;

	public ShellScreen(int sid, BshPM pm, BshIO io) : base(sid) {
		InputLine = "";
		LastPromptRow = 0;
		CursorPosition = 0;
	}

	public void Insert(char c) {
		InputLine = InputLine.Insert(CursorPosition, c.ToString());
		CursorPosition++;
		ValidateStates();
	}

	public void Insert(string str) {
		InputLine = InputLine.Insert(CursorPosition, str);
		CursorPosition += str.Length;
		ValidateStates();
	}

	public enum Action {
		Move,
		Delete,
	}

	public enum Motion {
		WordBack,
		WordForward,
		CharBack,
		CharForward,
		LineBack,
		LineForward,
	}

	public void Do(Action act, Motion motion) {
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
	}

	public int GetCursorDelta(Motion m) {
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
		// Not implemented
	}

	public void ClearInput() {
		InputLine = "";
		CursorPosition = 0;
	}
}

public class ProgramScreen : Screen {

	public ProgramScreen(int screenID) : base(screenID) {}

	public void Clear() {

	}
}
