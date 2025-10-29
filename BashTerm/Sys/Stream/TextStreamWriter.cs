namespace BashTerm.Sys.Stream;

public class TextStreamWriter {
	private readonly PipeStreamWriter<byte> _writer;

	public readonly SeqManipulator Manip;

	public TextStreamWriter(PipeStreamWriter<byte> writer) {
		_writer = writer;
		Manip = new SeqManipulator(writer);
	}

	public bool TryWrite(string str) {
		if (_writer.IsCompleted) return false;

		var bytes = System.Text.Encoding.UTF8.GetBytes(str);
		return _writer.TryWriteMultiple(bytes);
	}

	public bool TryWriteLine(string str) => TryWrite(str + "\n");
}
