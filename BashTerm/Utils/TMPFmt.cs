using System.Text;
using System.Text.RegularExpressions;

namespace BashTerm.Utils;

public class TMPLineParser {
	private int cols;

	public TMPLineParser(int cols) {
		this.cols = cols;
	}

	/*
	public List<string> GetLines(string text) {
		List<string> linesByCR = text.Split('\n').ToList();
		foreach (string line in linesByCR) {

		}
	}
	*/
}

internal class TMPFmt {
	private List<TMPTag> state;
	public TMPFmt() {

	}

	public bool Set(TMPTag tag) {
		state.Add(tag);
		return true;
	}

	public bool Unset(TMPTagType type) {
		for (int i = state.Count + 1; i >= 0; ++i) {
			if (state[i].type == type) {
				state.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	public string GetOpening() {
		StringBuilder sb = new();
		foreach (TMPTag tag in state) {
			sb.Append(tag);
		}
		return sb.ToString();
	}

	public string GetClosing() {
		StringBuilder sb = new();
		foreach (TMPTag tag in state) {
			sb.Append(tag.ToStringClosing());
		}
		return sb.ToString();
	}

	private static readonly Dictionary<string, TMPTagType> _tagNameToType = new(StringComparer.OrdinalIgnoreCase)
	{
		["align"] = TMPTagType.Align,
		["allcaps"] = TMPTagType.AllCaps,
		["alpha"] = TMPTagType.Alpha,
		["b"] = TMPTagType.Bold,
		["color"] = TMPTagType.Color,
		["cspace"] = TMPTagType.Cspace,
		// ["font"] = TMPTagType.Font, // Not supported
		["font-weight"] = TMPTagType.FontWeight,
		["gradient"] = TMPTagType.Gradient,
		["i"] = TMPTagType.Italic,
		["indent"] = TMPTagType.Indent,
		["lineheight"] = TMPTagType.LineHeight,
		["lineindent"] = TMPTagType.LineIndent,
		["link"] = TMPTagType.Link,
		["lowercase"] = TMPTagType.LowerCase,
		["margin"] = TMPTagType.Margin,
		["mark"] = TMPTagType.Mark,
		["mspace"] = TMPTagType.MSpace,
		["nobr"] = TMPTagType.NoBr,
		["noparse"] = TMPTagType.NoParse,
		["page"] = TMPTagType.Page,
		["pos"] = TMPTagType.Pos,
		["rotate"] = TMPTagType.Rotate,
		["s"] = TMPTagType.Strikethrough,
		["size"] = TMPTagType.Size,
		["sprite"] = TMPTagType.Sprite,
		["style"] = TMPTagType.Style,
		["sub"] = TMPTagType.Sub,
		["sup"] = TMPTagType.Sup,
		["u"] = TMPTagType.Underline,
		["uppercase"] = TMPTagType.UpperCase,
		["voffset"] = TMPTagType.VOffset,
		["width"] = TMPTagType.Width,
	};

	public static TMPTagType Txt2TagType(string tagName)
	{
		if (string.IsNullOrEmpty(tagName))
			throw new ArgumentException("tagName cannot be null or empty", nameof(tagName));

		if (_tagNameToType.TryGetValue(tagName.ToLowerInvariant(), out var result))
			return result;

		throw new KeyNotFoundException($"Unknown TMP tag name: {tagName}");
	}

	public static string TagType2Txt(TMPTagType type) => type switch
	{
		TMPTagType.Align => "align",
		TMPTagType.AllCaps => "allcaps",
		TMPTagType.Alpha => "alpha",
		TMPTagType.Bold => "b",
		TMPTagType.Color => "color",
		TMPTagType.Cspace => "cspace",
		// TMPTagType.Font => "font", // Not supported
		TMPTagType.FontWeight => "font-weight",
		TMPTagType.Gradient => "gradient",
		TMPTagType.Italic => "i",
		TMPTagType.Indent => "indent",
		TMPTagType.LineHeight => "lineheight",
		TMPTagType.LineIndent => "lineindent",
		TMPTagType.Link => "link",
		TMPTagType.LowerCase => "lowercase",
		TMPTagType.Margin => "margin",
		TMPTagType.Mark => "mark",
		TMPTagType.MSpace => "mspace",
		TMPTagType.NoBr => "nobr",
		TMPTagType.NoParse => "noparse",
		TMPTagType.Page => "page",
		TMPTagType.Pos => "pos",
		TMPTagType.Rotate => "rotate",
		TMPTagType.Strikethrough => "s",
		TMPTagType.Size => "size",
		TMPTagType.Sprite => "sprite",
		TMPTagType.Style => "style",
		TMPTagType.Sub => "sub",
		TMPTagType.Sup => "sup",
		TMPTagType.Underline => "u",
		TMPTagType.UpperCase => "uppercase",
		TMPTagType.VOffset => "voffset",
		TMPTagType.Width => "width",
	};

}

internal class TMPRegex {
	private const string pTagEndName = @"align|allcaps|b|color|cspace|font|font\-weight|gradient|i|indent|lineheight|lineindent|link|lowercase|margin|mark|mspace|nobr|noparse|page|pos|rotate|s|size|smallcaps|space|sprite|style|sub|sup|u|uppercase|voffset|width";
	private const string pTagName = pTagEndName + @"|alpha";
	private const string pTag = @"<(" + pTagName + @")(?:=([^>]+))?>";
	private const string pTagEnd = @"<\/(" + pTagEndName + ")>";

	private const string pAlignValue = @"^(left|right|center)$";
	private const string pColorName = @"^(black|blue|green|orange|purple|red|white|yellow)$";
	private const string pColorCode = @"^#([0-9A-Fa-f]{3,4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$";

	public static readonly Regex rTag = new(pTag, RegexOptions.Compiled | RegexOptions.IgnoreCase);
	public static readonly Regex rColorName =
		new(pColorName, RegexOptions.Compiled | RegexOptions.IgnoreCase);
	public static readonly Regex rColorCode =
		new(pColorCode, RegexOptions.Compiled);
}

internal record TMPTag(TMPTagType type, string value) {
	public override string ToString() {
		switch (type) {
			case TMPTagType.Align:
				return $"<align=\"{value}\">";
			case TMPTagType.AllCaps:
				return $"<allcaps>";
			case TMPTagType.Alpha:
				return $"<alpha=#{value}>";
			case TMPTagType.Bold:
				return $"<b>";
			case TMPTagType.Color:
				if (TMPRegex.rColorName.IsMatch(value))
					return $"<color={value}>";
				return $"<#{value}>";
			case TMPTagType.Cspace:
				// Any value is valid
				return $"<cspace={value}>";
			// case TMPTagType.Font:
			// 	return $"<font={value}>";
			case TMPTagType.FontWeight:
				return $"<font-weight={value}>";
			case TMPTagType.Gradient:
				return $"<gradient={value}";
			case TMPTagType.Italic:
			case TMPTagType.Indent:
			case TMPTagType.LineHeight:
			case TMPTagType.LineIndent:
			case TMPTagType.Link:
			case TMPTagType.LowerCase:
			case TMPTagType.Margin:
			case TMPTagType.Mark:
			case TMPTagType.MSpace:
			case TMPTagType.NoBr:
			case TMPTagType.NoParse:
			case TMPTagType.Page:
			case TMPTagType.Pos:
			case TMPTagType.Rotate:
			case TMPTagType.Strikethrough:
			case TMPTagType.Size:
			case TMPTagType.SmallCaps:
			case TMPTagType.Space:
			case TMPTagType.Sprite:
			case TMPTagType.Style:
			case TMPTagType.Sub:
			case TMPTagType.Sup:
			case TMPTagType.Underline:
			case TMPTagType.UpperCase:
			case TMPTagType.VOffset:
			case TMPTagType.Width:
				return $"<width={value}>";
			default:
				return "";
		}
	}

	public string ToStringClosing() {
		switch (type) {
			case TMPTagType.Align:
				return "</align>";
			case TMPTagType.AllCaps:
				return "</allcaps>";
			case TMPTagType.Alpha:
				return "<alpha=#FF>";
			case TMPTagType.Bold:
				return "</b>";
			case TMPTagType.Color:
				return "</color>";
			case TMPTagType.Cspace:
				return "</cspace>";
			// case TMPTagType.Font:
			// 	return "</font>";
			case TMPTagType.FontWeight:
				return "</font-weight>";
			case TMPTagType.Gradient:
				return "</gradient>";
			case TMPTagType.Italic:
			case TMPTagType.Indent:
			case TMPTagType.LineHeight:
			case TMPTagType.LineIndent:
			case TMPTagType.Link:
			case TMPTagType.LowerCase:
			case TMPTagType.Margin:
			case TMPTagType.Mark:
			case TMPTagType.MSpace:
			case TMPTagType.NoBr:
			case TMPTagType.NoParse:
			case TMPTagType.Page:
			case TMPTagType.Pos:
			case TMPTagType.Rotate:
			case TMPTagType.Strikethrough:
			case TMPTagType.Size:
			case TMPTagType.SmallCaps:
			case TMPTagType.Space:
			case TMPTagType.Sprite:
			case TMPTagType.Style:
			case TMPTagType.Sub:
			case TMPTagType.Sup:
			case TMPTagType.Underline:
			case TMPTagType.UpperCase:
			case TMPTagType.VOffset:
			case TMPTagType.Width:
				return $"</width>";
		}
		return "";
	}
}

internal enum TMPTagType {
	Align,
	AllCaps,
	Alpha,
	Bold,
	Color,
	Cspace,
	// Font, // We don't support this
	FontWeight,
	Gradient,
	Italic,
	Indent,
	LineHeight,
	LineIndent,
	Link,
	LowerCase,
	Margin,
	Mark,
	MSpace,
	NoBr,
	NoParse,
	Page,
	Pos,
	Rotate,
	Strikethrough,
	Size,
	SmallCaps,
	Space,
	Sprite,
	Style,
	Sub,
	Sup,
	Underline,
	UpperCase,
	VOffset,
	Width
}


