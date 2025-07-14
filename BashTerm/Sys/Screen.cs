namespace BashTerm.Sys;

public abstract class Screen {
	public const int Rows = 50; // TODO: Update to actual
	public int ScreenID { get; }
	public List<string> OutputHistory { get; }
	protected Queue<string> _outputQueue;
	public string DisplayBuffer {
		get { return _displayBuffer; }
	}
	protected string _displayBuffer;

	protected int _position;

	public Screen(int screenID) {
		ScreenID = screenID;
		OutputHistory = new List<string>();
		_outputQueue = new Queue<string>();
		_displayBuffer = "";
		_position = 0;
	}

	public void Print(string txt) {
		_outputQueue.Enqueue(txt);
	}

	public void Println(string txt) {
		Print(txt + '\n');
	}

	public void Println(List<string> lines) {
		foreach (var line in lines)
			_outputQueue.Enqueue(line + '\n');
	}

	public void Seek(int step) {
		_position = Math.Clamp(_position + step, 0, OutputHistory.Count - 1);
		int start = Math.Max(0, _position - Rows + 1);
		int left = Rows - (_position - start + 1);
		int end = Math.Min(OutputHistory.Count - 1, _position + left);
		_displayBuffer = string.Join('\n', OutputHistory.ToArray(), start, end - start + 1);
	}

}

public class BshScreen : Screen {
	public BshScreen(int screenID) : base(screenID) {}

	internal void Clear() {

	}
}

public class ProgramScreen : Screen {
	public ProgramScreen(int screenID) : base(screenID) {}

	public void Clear() {

	}
}
