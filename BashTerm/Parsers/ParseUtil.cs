using System.Text;
using BashTerm.Utils;
using Dissonance;

namespace BashTerm.Parsers;

internal static class ParseUtil {
	// NOTE: Used to convert references to commands in config to our internal enum
	private static readonly Dictionary<string, TermCmd> _Txt2Cmd = new Dictionary<string, TermCmd> {
		// Vanilla Operations
		{ "" , TermCmd.None },
		{ "help" , TermCmd.Help },
		{ "commands" , TermCmd.Commands },
		{ "cls" , TermCmd.Cls },
		{ "exit" , TermCmd.Exit },
		{ "override_lockdown" , TermCmd.Override },
		{ "deactivate_alarms" , TermCmd.DisableAlarm },
		{ "activate_beacon" , TermCmd.ActivateBeacon },
		{ "list" , TermCmd.ShowList },
		{ "query" , TermCmd.Query },
		{ "ping" , TermCmd.Ping },
		{ "reactor_startup" , TermCmd.ReactorStartup },
		{ "reactor_verify" , TermCmd.ReactorVerify },
		{ "reactor_shutdown" , TermCmd.ReactorShutdown },
		{ "uplink_connect" , TermCmd.TerminalUplinkConnect },
		{ "uplink_verify" , TermCmd.TerminalUplinkVerify },
		{ "uplink_confirm" , TermCmd.TerminalUplinkConfirm },
		{ "logs" , TermCmd.ListLogs },
		{ "read" , TermCmd.ReadLog },
		{ "info" , TermCmd.Info },

		// Custom Operations
		{ "raw" , TermCmd.Raw },
	};

	public static bool TryExpandAlias(string input, out string expansion) {
		Logger.Debug($"ExpandCmd: Got '{input}'");
		expansion = "";
		if (string.IsNullOrWhiteSpace(input)) {
			return false;
		}

		if (ConfigMaster.CmdExpExact.TryGetValue(input, out string? eps)) {
			Logger.Debug($"ExpandCmd: Returning (Alias) '{eps}'");
			expansion = eps;
			return true;
		}

		foreach (var tup in ConfigMaster.CmdExpPrefix) {
			if (input.StartsWith(tup.Prefix)) {
				Logger.Debug($"ExpandCmd: Returning (Alias) '{tup.Expansion}'");
				expansion = tup.Expansion;
				return true;
			}
		}

		Logger.Debug($"ExpandCmd: No change '{input}'");
		expansion = input;
		return false;
	}

	public static bool TryExpandObj(string objName, out string expansion) {
		expansion = "";
		if (string.IsNullOrWhiteSpace(objName)) {
			return false;
		}

		if (ConfigMaster.ObjExpExact.TryGetValue(objName, out string? eps)) {
			expansion = eps;
			return true;
		}

		foreach (var tup in ConfigMaster.ObjExpPrefix) {
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

	public static string[] CleanSplit(string input) {
		return input.Split(' ').Select(s => s.Trim()).ToArray();
	}

	public static string[] CleanSplit(char delimiter, string input) {
		return input.Split(delimiter).Select(s => s.Trim()).ToArray();
	}
}
