namespace BashTerm;

internal static class Styles {
	public static string C_Bsh => $"<#{_cBshHex}>";
	public static string C_Accent => $"<#{_cAccentHex}>";
	public static string C_Error => $"<#{_cErrorHex}>";
	public static string C_Warning => $"<#{_cWarningHex}>";
	public static string C_Info => $"<#{_cInfoHex}>";
	public static string C_Purple => $"<#{_cPurpleHex}>";
	public static string C_Weak => $"<#{_cWeakHex}>";
	public static string C_End => _cEnd;

	public static string M_Bsh => $"<mark=#{_mBshHex}>";
	public static string M_Accent => $"<mark=#{_mAccentHex}>";
	public static string M_End => _mEnd;

	private static readonly string _cBshHex = "FF77A9";
	private static readonly string _cAccentHex = "FF4400";
	private static readonly string _cErrorHex = "D01010";
	private static readonly string _cWarningHex = "CCD600";
	private static readonly string _cInfoHex = "10A010";
	private static readonly string _cPurpleHex = "581080";
	private static readonly string _cWeakHex = "888888";
	private static readonly string _cEnd = "</color>";

	private static readonly string _mBshHex = "FF91AF66";
	private static readonly string _mAccentHex = "FF33FF66";
	private static readonly string _mEnd = "</mark>";

	public static string Pos(int perc) {
		return $"<pos={Math.Clamp(perc, 0, 100)}%>";
	}

	public static string Indent(int perc) {
		return $"<indent={Math.Clamp(perc, 0, 100)}%>";
	}

	public static string EndPos = "</pos>";
	public static string EndIndent = "</indent>";
}
