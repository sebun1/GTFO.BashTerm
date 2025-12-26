using System.Buffers.Binary;
using Bsh.Types;

namespace Bsh.Sys.Stream;

/// <summary>
/// Provides a set of methods to manipulate terminal display using ANSI-like escape sequences.
/// </summary>
public class SeqManipulator : ISeqManip {
	private const byte Esc = 0x1b;
	private const byte Open = (byte)'[';

	private readonly bool Write2List;
	private readonly PipeStreamWriter<byte>? _writer;
	private readonly List<byte>? _outputList;

	public SeqManipulator(PipeStreamWriter<byte> writer) {
		_writer = writer;
		Write2List = false;
	}

	public SeqManipulator(List<byte> outputList) {
		_outputList = outputList;
		Write2List = true;
	}

	/**
	 * ==========================
	 * CURSOR MOVEMENT
	 * ==========================
	 */
	/// <summary>
	/// Moves the cursor up by max(1, n) positions.
	/// </summary>
	/// <param name="n">number of positions</param>
	/// <returns>true if write was successful</returns>
	public bool MoveCursorUp(UInt16 n = 1) {
		byte cmd = (byte)'A';
		if (n <= 1) return DoOp0(cmd);
		return DoOp16(cmd, n);
	}

	/// <summary>
	/// Moves the cursor down by max(1, n) positions.
	/// </summary>
	/// <param name="n">number of positions</param>
	/// <returns>true if write was successful</returns>
	public bool MoveCursorDown(UInt16 n = 1) {
		byte cmd = (byte)'B';
		if (n <= 1) return DoOp0(cmd);
		return DoOp16(cmd, n);
	}

	/// <summary>
	/// Moves the cursor right by max(1, n) positions.
	/// </summary>
	/// <param name="n">number of positions</param>
	/// <returns>true if write was successful</returns>
	public bool MoveCursorRight(UInt16 n = 1) {
		byte cmd = (byte)'C';
		if (n <= 1) return DoOp0(cmd);
		return DoOp16(cmd, n);
	}

	/// <summary>
	/// Moves the cursor left by max(1, n) positions.
	/// </summary>
	/// <param name="n">number of positions</param>
	/// <returns>true if write was successful</returns>
	public bool MoveCursorLeft(UInt16 n = 1) {
		byte cmd = (byte)'D';
		if (n <= 1) return DoOp0(cmd);
		return DoOp16(cmd, n);
	}

	/// <summary>
	/// Moves the cursor to the start of the line. max(1, n) lines down.
	/// </summary>
	/// <param name="n">number of lines</param>
	/// <returns>true if write was successful</returns>
	public bool MoveCursorStartOfNextLine(UInt16 n = 1) {
		byte cmd = (byte)'E';
		if (n <= 1) return DoOp0(cmd);
		return DoOp16(cmd, n);
	}

	/// <summary>
	/// Moves the cursor to the start of the line. max(1, n) lines up.
	/// </summary>
	/// <param name="n">number of lines</param>
	/// <returns>true if write was successful</returns>
	public bool MoveCursorStartOfPrevLine(UInt16 n = 1) {
		byte cmd = (byte)'F';
		if (n <= 1) return DoOp0(cmd);
		return DoOp16(cmd, n);
	}

	/// <summary>
	/// Moves the cursor to column n in the current row.
	/// </summary>
	/// <param name="n">column number</param>
	/// <returns>true if write was successful</returns>
	public bool MoveCursorToColumn(UInt16 n) {
		return DoOp16('G', n);
	}

	/// <summary>
	/// Moves the cursor to home position (0,0).
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool SetCursorHome() {
		return DoOp0('H');
	}

	/// <summary>
	/// Set the cursor position to (x,y).
	/// </summary>
	/// <param name="x">x (col) position</param>
	/// <param name="y">y (row) position</param>
	/// <returns>true if write was successful</returns>
	public bool SetCursor(UInt16 x, UInt16 y) {
		byte cmd = (byte)'H';
		Span<byte> xb = stackalloc byte[2];
		Span<byte> yb = stackalloc byte[2];
		BinaryPrimitives.WriteUInt16LittleEndian(xb, x);
		BinaryPrimitives.WriteUInt16LittleEndian(yb, y);
		if (((x >> 8) | (y >> 8)) != 0)
			return Write(Esc, Open, cmd, 4, xb[0], xb[1], yb[0], yb[1]);
		return Write(Esc, Open, cmd, 2, xb[0], yb[0]);
	}

	/**
	 * ==========================
	 * ERASE FUNCTIONS
	 * ==========================
	 */
	/// <summary>
	/// Erase from cursor to end of screen.
	/// i.e. From cursor to end of line, and all lines below.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool Erase2ScreenEnd() {
		return DoOp8('J', 0);
	}

	/// <summary>
	/// Erase from cursor to start of screen.
	/// i.e. From cursor to start of line, and all lines above.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool Erase2ScreenStart() {
		return DoOp8('J', 1);
	}

	/// <summary>
	/// Erase entire screen.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool EraseScreen() {
		return DoOp8('J', 2);
	}

	/// <summary>
	/// Erase from cursor to end of line.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool Erase2LineEnd() {
		return DoOp8('K', 0);
	}

	/// <summary>
	/// Erase from cursor to start of line.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool Erase2LineStart() {
		return DoOp8('K', 1);
	}

	/// <summary>
	/// Erase entire line.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool EraseLine() {
		return DoOp8('K', 2);
	}

	/**
	 * ==========================
	 * COLOR & GRAPHICS
	 * ==========================
	 */
	/// <summary>
	/// Sets the text color to the specified 8-bit RGB value.
	/// </summary>
	/// <param name="color">Color8 to be set to</param>
	/// <param name="fg">true to set foreground (text),
	/// false to set background (highlight)</param>
	/// <returns>true if write was successful</returns>
	public bool SetColor(Rgb8 color, bool fg = true) =>
		SetColor(color.R, color.G, color.B, fg);

	/// <summary>
	/// Sets the foreground (text) color to the specified 8-bit RGB value.
	/// </summary>
	/// <param name="r">red component (0-255)</param>
	/// <param name="g">green component (0-255)</param>
	/// <param name="b">blue component (0-255)</param>
	/// <param name="fg">true to set foreground (text),
	/// false to set background (highlight)</param>
	/// <returns>true if write was successful</returns>
	public bool SetColor(byte r, byte g, byte b, bool fg = true) {
		byte cmd = (byte)'m';
		if (fg)
			return Write(Esc, Open, cmd, 4, 0, r, g, b);
		return Write(Esc, Open, cmd, 4, 1, r, g, b);
	}

	/// <summary>
	/// Unsets the text color to default.
	/// Calls UnsetFgColor and UnsetBgColor.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool UnsetColor() {
		return UnsetFgColor() && UnsetBgColor();
	}


	/// <summary>
	/// Unsets the foreground (text) color to default.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool UnsetFgColor() {
		return DoOp8('m', 39);
	}

	/// <summary>
	/// Unsets the background (highlight) color to default.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool UnsetBgColor() {
		return DoOp8('m', 49);
	}

	/// <summary>
	/// Set bold text style.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool SetBold() {
		return DoOp8('m', 1);
	}

	/// <summary>
	/// Set non-bold text style.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool UnsetBold() {
		return DoOp8('m', 22);
	}

	/// <summary>
	/// Set italic text style.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool SetItalic() {
		return DoOp8('m', 3);
	}

	/// <summary>
	/// Set non-italic text style.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool UnsetItalic() {
		return DoOp8('m', 23);
	}

	/// <summary>
	/// Set underline text style.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool SetUnderline() {
		return DoOp8('m', 4);
	}

	/// <summary>
	/// Set non-underlined text style.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool UnsetUnderline() {
		return DoOp8('m', 24);
	}

	/// <summary>
	/// Set strikethrough text style.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool SetStrikethrough() {
		return DoOp8('m', 9);
	}

	/// <summary>
	/// Set normal (non-strikethrough) text style.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool UnsetStrikethrough() {
		return DoOp8('m', 29);
	}

	/// <summary>
	/// Resets the text color to default.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool ResetStyles() {
		return DoOp8('m', 0);
	}

	/*
	 * ==========================
	 * UTILITY
	 * ==========================
	 */
	private bool DoOp0(byte cmd) {
		return Write(Esc, Open, cmd, 0);
	}

	private bool DoOp0(char cmd) {
		return Write(Esc, Open, (byte)cmd, 0);
	}

	private bool DoOp8(byte cmd, byte n) {
		return Write(Esc, Open, cmd, 1, n);
	}

	private bool DoOp8(char cmd, byte n) {
		return Write(Esc, Open, (byte)cmd, 1, n);
	}

	private bool DoOp16(byte cmd, UInt16 n) {
		Span<byte> b = stackalloc byte[2];
		BinaryPrimitives.WriteUInt16LittleEndian(b, n);
		if ((n >> 8) != 0)
			return Write(Esc, Open, cmd, 2, b[0], b[1]);
		return Write(Esc, Open, cmd, 1, b[0]);
	}

	private bool DoOp16(char cmd, UInt16 n) {
		Span<byte> b = stackalloc byte[2];
		BinaryPrimitives.WriteUInt16LittleEndian(b, n);
		if ((n >> 8) != 0)
			return Write(Esc, Open, (byte)cmd, 2, b[0], b[1]);
		return Write(Esc, Open, (byte)cmd, 1, b[0]);
	}

	private bool Write(byte b0) {
		Span<byte> s = stackalloc byte[1];
		s[0] = b0;
		return Write(s);
	}

	private bool Write(byte b0, byte b1) {
		Span<byte> s = stackalloc byte[2];
		s[0] = b0;
		s[1] = b1;
		return Write(s);
	}

	private bool Write(byte b0, byte b1, byte b2) {
		Span<byte> s = stackalloc byte[3];
		s[0] = b0;
		s[1] = b1;
		s[2] = b2;
		return Write(s);
	}

	private bool Write(byte b0, byte b1, byte b2, byte b3) {
		Span<byte> s = stackalloc byte[4];
		s[0] = b0;
		s[1] = b1;
		s[2] = b2;
		s[3] = b3;
		return Write(s);
	}

	private bool Write(byte b0, byte b1, byte b2, byte b3, byte b4) {
		Span<byte> s = stackalloc byte[5];
		s[0] = b0;
		s[1] = b1;
		s[2] = b2;
		s[3] = b3;
		s[4] = b4;
		return Write(s);
	}

	private bool Write(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5) {
		Span<byte> s = stackalloc byte[6];
		s[0] = b0;
		s[1] = b1;
		s[2] = b2;
		s[3] = b3;
		s[4] = b4;
		s[5] = b5;
		return Write(s);
	}

	private bool Write(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6) {
		Span<byte> s = stackalloc byte[7];
		s[0] = b0;
		s[1] = b1;
		s[2] = b2;
		s[3] = b3;
		s[4] = b4;
		s[5] = b5;
		s[6] = b6;
		return Write(s);
	}

	private bool Write(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7) {
		Span<byte> s = stackalloc byte[8];
		s[0] = b0;
		s[1] = b1;
		s[2] = b2;
		s[3] = b3;
		s[4] = b4;
		s[5] = b5;
		s[6] = b6;
		s[7] = b7;
		return Write(s);
	}

	private bool Write(ReadOnlySpan<byte> bytes) {
		if (Write2List) {
			// write directly into the output list without allocating
			if (_outputList == null) return false;
			for (int i = 0; i < bytes.Length; i++) _outputList.Add(bytes[i]);
			return true;
		}

		if (_writer == null || _writer.IsCompleted) return false;

		// fallback: writer currently accepts byte[]; allocate once here
		byte[] arr = bytes.ToArray();
		return _writer.TryWriteMultiple(arr);
	}
}
