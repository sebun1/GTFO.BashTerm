namespace BashTerm;

public static class Styles {
	public static string Bashterm = "<#FF77A9>";     // Cute pink
	public static string Accent = "<#FF4400>";       // Warm orange accent
	public static string Error = "<#D11>";           // Red
	public static string Warning = "<#CCD600>";         // Yellow
	public static string Info = "<#1A1>";			 // Kinda Green
	public static string Purple = "<#581080>";      // Purple
	public static string CEnd = "</color>";

	public static string Pos(int perc) {
		return $"<pos={Math.Clamp(perc, 0, 100)}%>";
	}


	public static string Indent(int perc) {
		return $"<indent={Math.Clamp(perc, 0, 100)}%>";
	}

	public static string EndPos => "</pos>";
	public static string EndIndent => "</indent>";
}
