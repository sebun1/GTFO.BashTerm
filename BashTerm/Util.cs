using System.Collections.Generic;
using Il2CppSystem.Xml.Schema;

namespace BashTerm;

internal static class Util {
	// All methods assume and should have calls with lower case strings

	// NOTE: This is used to convert internal enum to the final GTFO command
	public static readonly Dictionary<TermCmd, string> _Cmd2Txt = new Dictionary<TermCmd, string> {
		// Vanilla Operations
		{ TermCmd.None, "" },
		{ TermCmd.Help, "help" },
		{ TermCmd.Commands, "commands" },
		{ TermCmd.Cls, "cls" },
		{ TermCmd.Exit, "exit" },
		{ TermCmd.Open, "" }, //?
		{ TermCmd.Close, "" }, //?
		{ TermCmd.Activate, "" }, //?
		{ TermCmd.Deactivate, "" }, //?
		{ TermCmd.EmptyLine, "" }, //?
		{ TermCmd.InvalidCommand, "" }, //?
		{ TermCmd.DownloadData, "" }, //?
		{ TermCmd.ViewSecurityLog, "" }, //?
		{ TermCmd.Override, "override_lockdown" },
		{ TermCmd.DisableAlarm, "deactivate_alarms" },
		{ TermCmd.Locate, "" }, //?
		{ TermCmd.ActivateBeacon, "activate_beacon" },
		{ TermCmd.Find, "" }, //?
		{ TermCmd.ShowList, "list" },
		{ TermCmd.Query, "query" },
		{ TermCmd.Ping, "ping" },
		{ TermCmd.ReactorStartup, "reactor_startup" },
		{ TermCmd.ReactorVerify, "reactor_verify" },
		{ TermCmd.ReactorShutdown, "reactor_shutdown" },
		{ TermCmd.WardenObjectiveSpecialCommand, "" }, //?
		{ TermCmd.TerminalUplinkConnect, "uplink_connect" },
		{ TermCmd.TerminalUplinkVerify, "uplink_verify" },
		{ TermCmd.TerminalUplinkConfirm, "uplink_confirm" },
		{ TermCmd.ListLogs, "logs" },
		{ TermCmd.ReadLog, "read" },
		{ TermCmd.TryUnlockingTerminal, "" }, //?
		{ TermCmd.WardenObjectiveGatherCommand, "" },
		{ TermCmd.TerminalCorruptedUplinkConnect, "uplink_verify" }, //?
		{ TermCmd.TerminalCorruptedUplinkVerify, "uplink_verify" }, //?
		{ TermCmd.UsedCommand, "" }, //?
		{ TermCmd.UniqueCommand1, "" }, //?
		{ TermCmd.UniqueCommand2, "" }, //?
		{ TermCmd.UniqueCommand3, "" }, //?
		{ TermCmd.UniqueCommand4, "" }, //?
		{ TermCmd.UniqueCommand5, "" }, //?
		{ TermCmd.Info, "info" },

		// Custom Operations
		{ TermCmd.ShowListU, "list u" },
		{ TermCmd.Raw, "raw" },
	};


	// NOTE: This is used to convert references to commandsl in config to our internal enum
	public static readonly Dictionary<string, TermCmd> _Txt2Cmd = new Dictionary<string, TermCmd> {
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
		{ "lsu" , TermCmd.ShowListU },
		{ "raw" , TermCmd.Raw },
	};

	// Old implementation of reversing the dictionary
	/*
	public static readonly Dictionary<string, TermCmd> _Txt2Cmd = _Cmd2Txt
		.Where(kvp => kvp.Value != "" || kvp.Key == TermCmd.None)
		.GroupBy(kvp => kvp.Value)
		.ToDictionary(g => g.Key, g => g.First().Key);
	*/

	public static string Cmd2Txt(TermCmd input) {
		string txt;
		return _Cmd2Txt.TryGetValue(input, out txt) ? txt : "";
	}

	public static TermCmd Txt2Cmd(string input) {
		TermCmd cmd;
		return _Txt2Cmd.TryGetValue(input, out cmd) ? cmd : TermCmd.None;
	}

	public static TermCmd InterpretCmd(string input) {
		if (string.IsNullOrEmpty(input)) {
			return TermCmd.None;
		}

		foreach (var kvp in ConfigMaster.CmdIdentifiers) {
			if (MatchAny(input, kvp.Value)) {
				return kvp.Key;
			}
		}

		return TermCmd.None;
	}

	public static string InterpretObject(string input) {
		if (string.IsNullOrEmpty(input)) {
			return "";
		}

		foreach (var kvp in ConfigMaster.ObjExpansions) {
			if (MatchAny(input, kvp.Value)) {
				return kvp.Key;
			}
		}

		return input;
	}

	public static bool MatchAny(string input, List<string> identifiers) {
		foreach (string id in identifiers)
			if (
				(id.EndsWith("+") && input.StartsWith(id.Substring(0, id.Length - 1)))
				|| input == id
			)
				return true;

		return false;
	}

	public static bool MatchAny(string input, HashSet<string> identifiers) {
		foreach (string id in identifiers)
			if (
				(id.EndsWith("+") && input.StartsWith(id.Substring(0, id.Length - 1)))
				|| input == id
			)
				return true;

		return false;
	}

	public static bool MatchAny(string input, params string[] identifiers) {
		foreach (string id in identifiers)
			if (
				(id.EndsWith("+") && input.StartsWith(id.Substring(0, id.Length - 1)))
				|| input == id
			)
				return true;
		return false;
	}

	public static string Concat(params string[] args) {
		return Concat(" ", args);
	}

	public static string Concat(string delimiter, params string[] args) {
		return Concat(delimiter, args, 0, args.Length);
	}

	public static string Concat(string[] args, int start, int end) {
		return Concat(" ", args, start, end);
	}

	public static string Concat(string delimiter, string[] args, int start, int end) {
		if (start < 0)
			start = 0;
		if (end > args.Length)
			end = args.Length;

		if (args.Length == 0 || start >= end)
			return "";

		string txt = args[start];
		for (int i = start + 1; i < end; i++)
			txt += delimiter + args[i];
		return txt;
	}

	public static string List2Str(List<string> lst, int max_count = -1) {
		if (max_count < 0 || max_count > lst.Count) max_count = lst.Count;
		return Concat(", ", lst.ToArray(), 0, max_count);
	}

	public static string Set2Str(HashSet<string> set, int max_count = -1) {
		int count = 0;
		bool first = true;
		string txt = "";
		foreach (string s in set) {
			if (max_count >= 0 && count >= max_count) break;
			count++;
			if (first) {
				first = false;
			} else {
				txt += ", ";
			}
			txt += s;
		}
		return txt;
	}

	public static bool ContainsFlag(params string[] args) {
		foreach (string arg in args)
			if (arg.StartsWith("-"))
				return true;
		return false;
	}

	public static bool IsInt(string input) {
		return int.TryParse(input, out int result);
	}
}
