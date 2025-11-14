using System.Buffers.Binary;
using Bsh.Sys.Stream;
using Il2CppSystem.Linq.Expressions.Interpreter;

namespace Bsh.Sys.Render.Parser;

public class TextStreamParser {
	public const uint DefaultParseLimit = 256;

	private const byte Esc = 0x1B;
	private const byte Open = (byte)'[';

	private PipeStreamReader<byte> _reader;
	private TextStreamReader _textReader;
	private Queue<TextStreamToken> _tokens;
	private uint _parseLimit = DefaultParseLimit;

	public TextStreamParser(PipeStreamReader<byte> reader) {
		_reader = reader;
		_textReader = new TextStreamReader(reader);
		_tokens = new Queue<TextStreamToken>();
	}

	public void SetParseLimit(uint limit) {
		_parseLimit = limit;
	}

	/// <summary>
	/// Parses up to <see cref="_parseLimit"/> tokens from the stream.
	/// </summary>
	/// <returns>Number of tokens parsed</returns>
	public uint Parse() {
		uint parsed = 0;
		while ((_parseLimit == 0 || parsed < _parseLimit) && _reader.Available) {
			try {
				_tokens.Enqueue(ParseOne());
			}
			catch (TextStreamParseException ex) {
				BshLogger.Error($"Error parsing text stream: {ex.Message}");
				return parsed;
			}
			catch (Exception ex) {
				BshLogger.Error($"Unexpected error while parsing text stream: {ex.Message}");
				return parsed;
			}

			parsed++;
		}

		return parsed;
	}

	private TextStreamToken ParseOne() {
		if (!_reader.Peek(out byte b))
			throw new TextStreamParseException("Unexpected end of stream");

		switch (b) {
			case (byte)'\n':
				_reader.TryRead(out _);
				return new TxtTokenLF();
			case (byte)'\r':
				_reader.TryRead(out _);
				return new TxtTokenCR();
			case (byte)'\t':
				_reader.TryRead(out _);
				return new TxtTokenTab();
			case Esc:
				return ParseEscapeSeq();
			default:
				return ParseString();
		}
	}

	private TextStreamToken ParseEscapeSeq() {
		if (!_reader.TryRead(out byte bEsc))
			throw new InvalidEscapeSequenceException("Sequence ended unexpectedly");
		if (bEsc != Esc)
			throw new InvalidEscapeSequenceException(
				"Expected ESC byte at start of escape sequence. Why am I trying parse an escape sequence right now?");

		if (!_reader.TryRead(out var bOpen))
			throw new InvalidEscapeSequenceException("Sequence ended unexpectedly after ESC");
		if (bOpen != Open)
			throw new InvalidEscapeSequenceException("ESC not followed by [");

		if (!_reader.TryRead(out var cmd))
			throw new InvalidEscapeSequenceException("Sequence ended unexpectedly after ESC[");

		if (!_reader.TryRead(out var paramCount))
			throw new InvalidEscapeSequenceException("Sequence ended unexpectedly after ESC[<cmd>");

		Span<byte> paramBytes = stackalloc byte[paramCount];
		if (!_reader.TryReadMultiple(paramCount, paramBytes))
			throw new InvalidEscapeSequenceException(
				$"Sequence ended unexpectedly while reading parameters, expected {paramCount} bytes");

		switch (cmd) {
			// Movement
			case (byte)'A':
				return new TxtTokenMoveCursor(0, -GetValueFromBytes(paramBytes));
			case (byte)'B':
				return new TxtTokenMoveCursor(0, GetValueFromBytes(paramBytes));
			case (byte)'C':
				return new TxtTokenMoveCursor(GetValueFromBytes(paramBytes), 0);
			case (byte)'D':
				return new TxtTokenMoveCursor(-GetValueFromBytes(paramBytes), 0);
			case (byte)'E':
				return new TxtTokenMoveCursorStartOfLine(GetValueFromBytesOrDefault(paramBytes, 1));
			case (byte)'F':
				return new TxtTokenMoveCursorStartOfLine(-GetValueFromBytesOrDefault(paramBytes, 1));
			case (byte)'G':
				return new TxtTokenSetCursorColumn(GetValueFromBytes(paramBytes));
			case (byte)'H':
				return paramCount switch {
					0 => new TxtTokenSetCursor(0, 0),
					2 => new TxtTokenSetCursor(paramBytes[0], paramBytes[1]),
					4 => new TxtTokenSetCursor(BinaryPrimitives.ReadUInt16LittleEndian(paramBytes.Slice(0, 2)),
						BinaryPrimitives.ReadUInt16LittleEndian(paramBytes.Slice(2, 2))),
					_ => throw new InvalidEscapeSequenceException(
						$"Invalid parameter count for command H:SetCursor, expected 0, 2, or 4, got {paramCount}")
				};

			// Erase
			case (byte)'J':
				return paramBytes[0] switch {
					0 => new TxtTokenEraseToEnd(),
					1 => new TxtTokenEraseToStart(),
					2 => new TxtTokenEraseScreen(),
					_ => throw new InvalidEscapeSequenceException(
						$"Invalid parameter for command J:Erase, expected 0, 1, or 2, got {paramBytes[0]}")
				};
			case (byte)'K':
				return paramBytes[0] switch {
					0 => new TxtTokenEraseToLineEnd(),
					1 => new TxtTokenEraseToLineStart(),
					2 => new TxtTokenEraseLine(),
					_ => throw new InvalidEscapeSequenceException(
						$"Invalid parameter for command K:EraseInline, expected 0, 1, or 2, got {paramBytes[0]}")
				};

			// Color & Graphics
			case (byte)'m':
				if (paramCount == 4) {
					if (paramBytes[0] == 0x00)
						return new TxtTokenSetFgColor(paramBytes[1], paramBytes[2], paramBytes[3]);
					if (paramBytes[0] == 0x01)
						return new TxtTokenSetBgColor(paramBytes[1], paramBytes[2], paramBytes[3]);
					throw new InvalidEscapeSequenceException(
						$"Invalid first parameter for 4-parameter command m:Graphics, expected 0 or 1, got {paramBytes[0]}");
				}

				if (paramCount != 1)
					throw new InvalidEscapeSequenceException(
						$"Invalid parameter count for command m:Graphics, expected 1 or 3, got {paramCount}");

				return paramBytes[0] switch {
					0 => new TxtTokenResetStyles(),
					39 => new TxtTokenUnsetFgColor(),
					49 => new TxtTokenUnsetBgColor(),
					1 => new TxtTokenSetBold(),
					22 => new TxtTokenUnsetBold(),
					3 => new TxtTokenSetItalic(),
					23 => new TxtTokenUnsetItalic(),
					4 => new TxtTokenSetUnderline(),
					24 => new TxtTokenUnsetUnderline(),
					9 => new TxtTokenSetStrikethrough(),
					29 => new TxtTokenUnsetStrikethrough(),
					_ => throw new InvalidEscapeSequenceException(
						$"Unknown parameter for command m:graphics, got {paramBytes[0]}")
				};

			default:
				throw new InvalidEscapeSequenceException($"Unknown command byte: {(char)cmd}");
		}
	}

	private TextStreamToken ParseString() {
		{
			if (_textReader.TryReadStringToken(out string s)) {
				return new TxtTokenText(s);
			}
		}

		List<byte> bytes = _reader.ReadAll();
		try {
			return new TxtTokenText(System.Text.Encoding.UTF8.GetString(bytes.ToArray()));
		}
		catch (ArgumentException argE) {
			throw new TextStreamParseException("Failed to decode string token: " + argE.Message);
		}
	}

	public bool Get(out TextStreamToken? token) {
		return _tokens.TryDequeue(out token);
	}

	private UInt16 GetValueFromBytesOrDefault(Span<byte> bytes, UInt16 defaultValue = 0) {
		if (bytes.Length == 0)
			return defaultValue;
		return GetValueFromBytes(bytes);
	}

	private UInt16 GetValueFromBytes(Span<byte> bytes) {
		if (bytes.Length == 1)
			return bytes[0];
		if (bytes.Length == 2)
			return BinaryPrimitives.ReadUInt16LittleEndian(bytes);
		throw new InvalidEscapeSequenceException(
			$"Cannot parse value from {bytes.Length} bytes, expected 1 or 2 bytes");
	}
}

public class TextStreamParseException : Exception {
	public TextStreamParseException(string message) : base($"[TextStreamParse] >> {message}") {
	}
}

public class InvalidEscapeSequenceException : TextStreamParseException {
	public InvalidEscapeSequenceException(string sequence)
		: base($"[BadEscSeq] >> {sequence}") {
	}
}
