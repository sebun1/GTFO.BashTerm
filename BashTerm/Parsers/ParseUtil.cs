using System.Text;
using Bsh.Utils;
using Bsh.Sys;
using Dissonance;

namespace Bsh.Parsers;

internal static class ParseUtil {
	public static bool TryExpandAlias(string input, out string expansion) {
		BshLogger.Debug($"ExpandCmd: Got '{input}'");
		expansion = "";
		if (string.IsNullOrWhiteSpace(input)) {
			return false;
		}

		if (Config.CmdExpExact.TryGetValue(input, out string? eps)) {
			BshLogger.Debug($"ExpandCmd: Returning (Alias) '{eps}'");
			expansion = eps;
			return true;
		}

		foreach (var tup in Config.CmdExpPrefix) {
			if (input.StartsWith(tup.Prefix)) {
				BshLogger.Debug($"ExpandCmd: Returning (Alias) '{tup.Expansion}'");
				expansion = tup.Expansion;
				return true;
			}
		}

		BshLogger.Debug($"ExpandCmd: No change '{input}'");
		expansion = input;
		return false;
	}

	public static bool TryExpandObj(string objName, out string expansion) {
		expansion = "";
		if (string.IsNullOrWhiteSpace(objName)) {
			return false;
		}

		if (Config.ObjExpExact.TryGetValue(objName, out string? eps)) {
			expansion = eps;
			return true;
		}

		foreach (var tup in Config.ObjExpPrefix) {
			if (objName.StartsWith(tup.Prefix)) {
				expansion = tup.Expansion;
				return true;
			}
		}

		expansion = objName;
		return false;
	}

	/// <summary>
	/// Parse User Definition Groups with ":" as group separator and "," as term separator.
	/// Trims whitespaces in all terms. "\" will escape next char (take literally).
	/// </summary>
	/// <param name="input">Input to be parsed</param>
	/// <returns>Parsed groups</returns>
	public static List<List<string>> GetAliasGroups(string input) {
		input = input.ToLower();

		StringBuilder sb = new StringBuilder();
		List<List<string>> groups = new List<List<string>>();
		List<string> group = new List<string>();
		bool escapeNext = false;

		foreach (char c in input) {
			if (escapeNext) {
				sb.Append(c);
				escapeNext = false;
			} else if (c == '\\') {
				escapeNext = true;
			} else if (c == ',') {
				group.Add(sb.ToString().Trim());
				sb.Clear();
			} else if (c == ':') {
				group.Add(sb.ToString().Trim());
				groups.Add(group);
				group = new List<string>();
				sb.Clear();
			} else {
				sb.Append(c);
			}
		}

		group.Add(sb.ToString().Trim());
		groups.Add(group);

		return groups;
	}
}
