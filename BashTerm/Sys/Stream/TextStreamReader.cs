namespace Bsh.Sys.Stream;

public class TextStreamReader {
	private static readonly byte[] NewlineBytes = { (byte)'\n', (byte)'\r' };
	private static readonly byte[] WhitespaceBytes = { (byte)' ', (byte)'\t', (byte)'\n', (byte)'\r' };

	private static readonly byte[]
		NonStringTokenBytes = { (byte)'\t', (byte)'\n', (byte)'\r', 0x1B }; // this is for internal use

	private readonly PipeStreamReader<byte> _reader;

	/// <summary>
	/// Creates a new TextStreamReader instance for text-based reading from a
	/// byte-based PipeStreamReader.
	/// </summary>
	/// <param name="reader">byte-based PipeStreamReader to read from</param>
	public TextStreamReader(PipeStreamReader<byte> reader) {
		_reader = reader;
	}

	/// <summary>
	/// Tries to read from the stream until the specified byte character is encountered
	/// </summary>
	/// <param name="c">char to look for</param>
	/// <param name="result">string read if successful</param>
	/// <param name="consumeChar">whether the read should consume the target char</param>
	/// <returns>true if read was successful</returns>
	public bool TryReadToByteChar(byte c, out string result, bool consumeChar = true) {
		result = "";
		int targetIdx = -1;

		int i = 0;
		foreach (var b in _reader.Owner.Queue) {
			if (b == c) {
				targetIdx = i;
				break;
			}

			i++;
		}

		if (targetIdx == -1) return false;

		if (consumeChar) targetIdx++;

		Span<byte> bytes = stackalloc byte[targetIdx];

		if (!_reader.TryReadMultiple(targetIdx, bytes)) return false;

		try {
			result = System.Text.Encoding.UTF8.GetString(bytes).TrimEnd((char)c);
		}
		catch (ArgumentException e) {
			return false;
		}
		catch (Exception e) {
			BepLogger.Error($"{e.Message}");
			return false;
		}

		return true;
	}

	/// <summary>
	/// Tries to read from the stream until any of the specified byte characters is encountered
	/// </summary>
	/// <param name="chars">array of char candidates</param>
	/// <param name="result">string read if successful</param>
	/// <param name="consumeChar">whether the read should consume the target char</param>
	/// <returns>true if read was successful</returns>
	public bool TryReadToAnyByteChar(byte[] chars, out string result, bool consumeChar = true) {
		result = "";
		int targetIdx = -1;

		int i = 0;
		foreach (var b in _reader.Owner.Queue) {
			if (MatchAny(b, chars)) {
				targetIdx = i;
				break;
			}

			i++;
		}

		if (targetIdx == -1) return false;

		if (consumeChar) targetIdx++;

		Span<byte> bytes = stackalloc byte[targetIdx];

		if (!_reader.TryReadMultiple(targetIdx, bytes)) return false;

		try {
			result = System.Text.Encoding.UTF8.GetString(bytes).TrimEnd(AsCharArray(chars));
		}
		catch (ArgumentException e) {
			return false;
		}
		catch (Exception e) {
			BepLogger.Error($"{e.Message}");
			return false;
		}

		return true;
	}

	/// <summary>
	/// Tries to read a line from the stream.
	/// (reads until newline or carriage return i.e. '\n' or '\r')
	/// Consumes the newline/carriage return character.
	/// </summary>
	/// <param name="line">line read if successful</param>
	/// <returns>if read was successful</returns>
	public bool TryReadLine(out string line) {
		return TryReadToAnyByteChar(NewlineBytes, out line);
	}

	/// <summary>
	/// Tries to read a word from the stream.
	/// (reads until the next ' ', '\t', '\n' or '\r' character)
	/// Consumes the whitespace character.
	/// </summary>
	/// <param name="line">line read if successful</param>
	/// <returns>if read was successful</returns>
	public bool TryReadWord(out string word) {
		return TryReadToAnyByteChar(WhitespaceBytes, out word);
	}

	internal bool TryReadStringToken(out string str) {
		return TryReadToAnyByteChar(NonStringTokenBytes, out str, consumeChar: false);
	}

	private bool MatchAny(byte b, byte[] bytes) {
		foreach (var by in bytes) {
			if (b == by) return true;
		}

		return false;
	}

	private char[] AsCharArray(byte[] bytes) {
		char[] chars = new char[bytes.Length];
		for (int i = 0; i < bytes.Length; i++) {
			chars[i] = (char)bytes[i];
		}

		return chars;
	}
}
