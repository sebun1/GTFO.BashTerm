namespace Bsh.Sys.Render.History;

[Flags]
public enum RunType : UInt16 {
	FgColor = 1 << 0,
	BgColor = 1 << 1,
	Bold = 1 << 2,
	Italic = 1 << 3,
	Underline = 1 << 4,
	Strikethrough = 1 << 5,
	Color = FgColor | BgColor
}
