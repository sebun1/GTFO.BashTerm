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
				return new TokenLF();
			case (byte)'\r':
				_reader.TryRead(out _);
				return new TokenCR();
			case (byte)'\t':
				_reader.TryRead(out _);
				return new TokenTab();
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
				return new TokenMoveCursor(0, -GetValueFromBytes(paramBytes));
			case (byte)'B':
				return new TokenMoveCursor(0, GetValueFromBytes(paramBytes));
			case (byte)'C':
				return new TokenMoveCursor(GetValueFromBytes(paramBytes), 0);
			case (byte)'D':
				return new TokenMoveCursor(-GetValueFromBytes(paramBytes), 0);
			case (byte)'E':
				return new TokenMoveCursorStartOfLine(GetValueFromBytesOrDefault(paramBytes, 1));
			case (byte)'F':
				return new TokenMoveCursorStartOfLine(-GetValueFromBytesOrDefault(paramBytes, 1));
			case (byte)'G':
				return new TokenSetCursorColumn(GetValueFromBytes(paramBytes));
			case (byte)'H':
				return paramCount switch {
					0 => new TokenSetCursor(0, 0),
					2 => new TokenSetCursor(paramBytes[0], paramBytes[1]),
					4 => new TokenSetCursor(BinaryPrimitives.ReadUInt16LittleEndian(paramBytes.Slice(0, 2)),
						BinaryPrimitives.ReadUInt16LittleEndian(paramBytes.Slice(2, 2))),
					_ => throw new InvalidEscapeSequenceException(
						$"Invalid parameter count for command H:SetCursor, expected 0, 2, or 4, got {paramCount}")
				};

			// Erase
			case (byte)'J':
				return paramBytes[0] switch {
					0 => new TokenEraseToEnd(),
					1 => new TokenEraseToStart(),
					2 => new TokenEraseScreen(),
					_ => throw new InvalidEscapeSequenceException(
						$"Invalid parameter for command J:Erase, expected 0, 1, or 2, got {paramBytes[0]}")
				};
			case (byte)'K':
				return paramBytes[0] switch {
					0 => new TokenEraseToLineEnd(),
					1 => new TokenEraseToLineStart(),
					2 => new TokenEraseLine(),
					_ => throw new InvalidEscapeSequenceException(
						$"Invalid parameter for command K:EraseInline, expected 0, 1, or 2, got {paramBytes[0]}")
				};

			// Color & Graphics
			case (byte)'m':
				if (paramCount == 3)
					return new TokenSetColor(paramBytes[0], paramBytes[1], paramBytes[2]);

				if (paramCount != 1)
					throw new InvalidEscapeSequenceException(
						$"Invalid parameter count for command m:Graphics, expected 1 or 3, got {paramCount}");

				return paramBytes[0] switch {
					0 => new TokenResetStyles(),
					39 => new TokenUnsetColor(),
					2 => new TokenSetBold(),
					22 => new TokenUnsetBold(),
					4 => new TokenSetUnderline(),
					24 => new TokenUnsetUnderline(),
					9 => new TokenSetStrikethrough(),
					29 => new TokenUnsetStrikethrough(),
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
				return new TokenText(s);
			}
		}

		List<byte> bytes = _reader.ReadAll();
		try {
			return new TokenText(System.Text.Encoding.UTF8.GetString(bytes.ToArray()));
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
			$"Cannot parse value from {bytes.Length} bytes, only 1 or 2 bytes supported");
	}
}

public class TextStreamParseException : Exception {
	public TextStreamParseException(string message) : base(message) {
	}
}

public class InvalidEscapeSequenceException : TextStreamParseException {
	public InvalidEscapeSequenceException(string sequence)
		: base($"Invalid escape sequence: {sequence}") {
	}
}
