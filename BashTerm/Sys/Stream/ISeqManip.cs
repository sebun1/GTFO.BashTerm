using Bsh.Types;

namespace Bsh.Sys.Stream;

/// <summary>
/// Escape sequence manipulator interface (terminal control & styling).
/// All methods return true if the underlying write succeeds; false if the writer is completed / closed.
/// </summary>
public interface ISeqManip {
	// Cursor movement
	bool MoveCursorUp(UInt16 n = 1);
	bool MoveCursorDown(UInt16 n = 1);
	bool MoveCursorRight(UInt16 n = 1);
	bool MoveCursorLeft(UInt16 n = 1);
	bool MoveCursorStartOfNextLine(UInt16 n = 1);
	bool MoveCursorStartOfPrevLine(UInt16 n = 1);
	bool MoveCursorToColumn(UInt16 n);
	bool SetCursorHome();
	bool SetCursor(UInt16 x, UInt16 y);

	// Erase functions
	bool Erase2ScreenEnd();
	bool Erase2ScreenStart();
	bool EraseScreen();
	bool Erase2LineEnd();
	bool Erase2LineStart();
	bool EraseLine();

	// Color & graphics
	bool SetColor(Rgb8 color, bool fg = true);
	bool SetColor(byte r, byte g, byte b, bool fg = true);
	bool UnsetFgColor();
	bool UnsetBgColor();

	// Composite unset (foreground + background).
	bool UnsetColor();

	// Styles
	bool SetBold();
	bool UnsetBold();
	bool SetItalic();
	bool UnsetItalic();
	bool SetUnderline();
	bool UnsetUnderline();
	bool SetStrikethrough();
	bool UnsetStrikethrough();
	bool ResetStyles();
}
