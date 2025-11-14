namespace Bsh.Sys.Stream;

public class TextStreamWriter {
	private readonly PipeStreamWriter<byte> _writer;

	/// <summary>
	/// Manipulator for writing escape sequences to the stream.
	/// </summary>
	public readonly ISeqManip Manip;

	/// <summary>
	/// Creates a new TextStreamWriter instance for writing text to a byte stream.
	/// </summary>
	/// <param name="writer"></param>
	public TextStreamWriter(PipeStreamWriter<byte> writer) {
		_writer = writer;
		Manip = new SeqManipulator(writer);
	}

	/// <summary>
	/// Tries to write a string to the stream.
	/// </summary>
	/// <param name="str">string to write</param>
	/// <returns>true if write was successful</returns>
	public bool TryWrite(string str) {
		if (_writer.IsCompleted) return false;

		var bytes = System.Text.Encoding.UTF8.GetBytes(str);
		return _writer.TryWriteMultiple(bytes);
	}

	/// <summary>
	/// Tries to write a line (string + newline) to the stream.
	/// </summary>
	/// <param name="str">string to write</param>
	/// <returns>true if write was successful</returns>
	public bool TryWriteLine(string str) => TryWrite(str + "\n");
}
