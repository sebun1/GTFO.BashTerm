using Localization;

namespace BashTerm.Sys.Render.Parser;

public abstract record TextStreamToken;

// Standard Escape Sequences
public record TokenText(string Text) : TextStreamToken {
	public override string ToString() => Text;

	public TokenText Join(TokenText next) {
		return new TokenText(Text + next.Text);
	}
};

public record TokenLF : TextStreamToken;

public record TokenCR : TextStreamToken;

public record TokenTab : TextStreamToken;

// Cursor Movement
public record TokenMoveCursor(int X, int Y) : TextStreamToken;

public record TokenMoveCursorStartOfLine(int offset) : TextStreamToken;

public record TokenSetCursorColumn(int X) : TextStreamToken;

public record TokenSetCursor(int X, int Y) : TextStreamToken;

// Erase
public record TokenEraseToStart : TextStreamToken;

public record TokenEraseToEnd : TextStreamToken;

public record TokenEraseScreen : TextStreamToken;

public record TokenEraseToLineStart : TextStreamToken;

public record TokenEraseToLineEnd : TextStreamToken;

public record TokenEraseLine : TextStreamToken;

// Style & Graphics
public record TokenSetColor(byte R, byte G, byte B) : TextStreamToken;

public record TokenUnsetColor : TextStreamToken;

public record TokenSetBold : TextStreamToken;

public record TokenUnsetBold : TextStreamToken;

public record TokenSetUnderline : TextStreamToken;

public record TokenUnsetUnderline : TextStreamToken;

public record TokenSetStrikethrough : TextStreamToken;

public record TokenUnsetStrikethrough : TextStreamToken;

public record TokenResetStyles : TextStreamToken;
