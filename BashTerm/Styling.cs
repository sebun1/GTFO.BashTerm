namespace BashTerm;

public static class Styling {
	public static string Bashterm = "<#FF77A9>";     // Cute pink
	public static string Accent = "<#FFB347>";       // Warm orange accent
	public static string Error = "<#D11>";           // Red
	public static string Warning = "<#B11>";         // Orange-ish red
	public static string Playful = "<#C084FC>";      // Playful purple (soft lavender/violet)
	public static string End = "</color>";
}

public static class Fmt {
	public static string Pos(int perc) {
		return $"<pos={Math.Clamp(perc, 0, 100)}%>";
	}


	public static string Indent(int perc) {
		return $"<indent={Math.Clamp(perc, 0, 100)}%>";
	}

	public static string EndPos => "</pos>";
	public static string EndIndent => "</indent>";
}
