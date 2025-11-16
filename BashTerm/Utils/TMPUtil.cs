using Bsh.Sys.Render.History;

namespace Bsh.Utils;

public static class TMPUtil {
	public const float TMP_FG_ALPHA = 1.0f;
	public const float TMP_BG_ALPHA = 0.02f;
	public const byte TEXT_FG_ALPHA = 16;
	public const byte TEXT_BG_ALPHA = 64;

	public static string OpenTag(RunType type, string param = "") {
		return type switch {
			RunType.FgColor => $"<#{param}>",
			RunType.BgColor => $"<mark=#{ExtendColor(param)}>",
			RunType.Bold => "<b>",
			RunType.Italic => "<i>",
			RunType.Underline => "<u>",
			RunType.Strikethrough => "<s>",
			_ => ""
		};
	}

	public static string CloseTag(RunType type) {
		return type switch {
			RunType.FgColor => "</color>",
			RunType.BgColor => "</mark>",
			RunType.Bold => "</b>",
			RunType.Italic => "</i>",
			RunType.Underline => "</u>",
			RunType.Strikethrough => "</s>",
			_ => ""
		};
	}

	private static string ExtendColor(string str) {
		if (str.Length == 3)
			return $"{str[0]}F{str[1]}F{str[2]}F";
		return str;
	}
}
