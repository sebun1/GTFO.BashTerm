using System.Buffers.Binary;

namespace BashTerm.Sys.Stream;

/// <summary>
/// Provides a set of methods to manipulate terminal display using ANSI-like escape sequences.
/// </summary>
public class SeqManipulator {
	private const byte Esc = 0x1b;
	private const byte Open = (byte)'[';

	private readonly PipeStreamWriter<byte> _writer;

	public SeqManipulator(PipeStreamWriter<byte> writer) {
		_writer = writer;
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
	/// <param name="x">x position</param>
	/// <param name="y">y position</param>
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
	/// <param name="r">red component (0-255)</param>
	/// <param name="g">green component (0-255)</param>
	/// <param name="b">blue component (0-255)</param>
	/// <returns>true if write was successful</returns>
	public bool SetColor(byte r, byte g, byte b) {
		byte cmd = (byte)'m';
		return Write(Esc, Open, cmd, 3, r, g, b);
	}

	/// <summary>
	/// Unsets the text color to default.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool UnsetColor() {
		return DoOp8('m', 39);
	}

	/// <summary>
	/// Set bold text style.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool SetBold() {
		return DoOp8('m', 2);
	}

	/// <summary>
	/// Set normal (non-bold) text style.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool UnsetBold() {
		return DoOp8('m', 22);
	}

	/// <summary>
	/// Set underline text style.
	/// </summary>
	/// <returns>true if write was successful</returns>
	public bool SetUnderline() {
		return DoOp8('m', 4);
	}

	/// <summary>
	/// Set normal (non-underlined) text style.
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

	/**
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
			return Write(Esc, Open, cmd, 3, b[0], b[1]);
		return Write(Esc, Open, cmd, 1, b[0]);
	}

	private bool DoOp16(char cmd, UInt16 n) {
		Span<byte> b = stackalloc byte[2];
		BinaryPrimitives.WriteUInt16LittleEndian(b, n);
		if ((n >> 8) != 0)
			return Write(Esc, Open, (byte)cmd, 2, b[0], b[1]);
		return Write(Esc, Open, (byte)cmd, 1, b[0]);
	}

	private bool Write(params byte[] bytes) {
		if (_writer.IsCompleted) return false;
		return _writer.TryWriteMultiple(bytes);
	}
}
