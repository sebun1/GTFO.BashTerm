using Localization;

namespace Bsh.Sys.Render.Parser;

public abstract record TextStreamToken;

// Standard Escape Sequences
public record TxtTokenText(string Text) : TextStreamToken {
	public override string ToString() => Text;

	public TxtTokenText Join(TxtTokenText next) {
		return new TxtTokenText(Text + next.Text);
	}
};

public record TxtTokenLF : TextStreamToken;

public record TxtTokenCR : TextStreamToken;

public record TxtTokenTab : TextStreamToken;

// Cursor Movement
public record TxtTokenMoveCursor(int X, int Y) : TextStreamToken;

public record TxtTokenMoveCursorStartOfLine(int offset) : TextStreamToken;

public record TxtTokenSetCursorColumn(int X) : TextStreamToken;

public record TxtTokenSetCursor(int X, int Y) : TextStreamToken;

// Erase
public record TxtTokenEraseToStart : TextStreamToken;

public record TxtTokenEraseToEnd : TextStreamToken;

public record TxtTokenEraseScreen : TextStreamToken;

public record TxtTokenEraseToLineStart : TextStreamToken;

public record TxtTokenEraseToLineEnd : TextStreamToken;

public record TxtTokenEraseLine : TextStreamToken;

// Style & Graphics
public record TxtTokenSetFgColor(byte R, byte G, byte B) : TextStreamToken;

public record TxtTokenUnsetFgColor : TextStreamToken;

public record TxtTokenSetBgColor(byte R, byte G, byte B) : TextStreamToken;

public record TxtTokenUnsetBgColor : TextStreamToken;

public record TxtTokenSetBold : TextStreamToken;

public record TxtTokenUnsetBold : TextStreamToken;

public record TxtTokenSetItalic : TextStreamToken;

public record TxtTokenUnsetItalic : TextStreamToken;

public record TxtTokenSetUnderline : TextStreamToken;

public record TxtTokenUnsetUnderline : TextStreamToken;

public record TxtTokenSetStrikethrough : TextStreamToken;

public record TxtTokenUnsetStrikethrough : TextStreamToken;

public record TxtTokenResetStyles : TextStreamToken;
