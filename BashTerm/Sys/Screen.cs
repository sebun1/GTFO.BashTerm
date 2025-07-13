namespace BashTerm.Sys;

public class Screen {
	public const int Rows = 50; // TODO: Update to actual
	public int ScreenID { get; }
	public List<string> OutputHistory { get; }
	private Queue<string> _outputQueue;
	private string _displayBuffer;

	private int _position;

	public Screen(int screenID) {
		ScreenID = screenID;
		OutputHistory = new List<string>();
		_outputQueue = new Queue<string>();
		_displayBuffer = "";
	}

	public void Attach() {

	}

	public void Append(string str) {
		_outputQueue.Enqueue(str);
	}

	public void Append(List<string> strs) {
		foreach (var str in strs)
			_outputQueue.Enqueue(str);
	}

	public void Rewind(int step) {
		Seek(0 - step);
	}

	public void Seek(int step) {
		_position = Math.Clamp(_position + step, 0, OutputHistory.Count - 1);
		int start = Math.Max(0, _position - Rows + 1);
		int left = Rows - (_position - start + 1);
		int end = Math.Min(OutputHistory.Count - 1, _position + left);
		_displayBuffer = string.Join('\n', OutputHistory.ToArray(), start, end - start + 1);
	}
}
