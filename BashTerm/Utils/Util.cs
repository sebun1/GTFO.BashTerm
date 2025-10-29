using BashTerm.Sys;
using UnityEngine;
using Regex = System.Text.RegularExpressions.Regex;

namespace BashTerm.Utils;

internal static class Util {
	public static string Concat(params string[] args) {
		return Concat(' ', args);
	}

	public static string Concat(char delimiter, params string[] args) {
		return Concat(delimiter, args, 0, args.Length);
	}

	public static string Concat(string[] args, int start, int end) {
		return Concat(' ', args, start, end);
	}

	public static string Concat(char delimiter, string[] args, int start, int end) {
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

	public static bool IsInt(string input) {
		return int.TryParse(input, out _);
	}

	public static void printMaps() {
		BepLogger.Info("CmdExpExact:");
		foreach (var pair in Config.CmdExpExact) {
			BepLogger.Info("\t" + pair.Key + " -> " + pair.Value);
		}

		BepLogger.Info("CmdExpPrefix:");
		foreach (var tup in Config.CmdExpPrefix) {
			BepLogger.Info("\t" + tup.Prefix + "+ -> " + tup.Expansion);
		}

		BepLogger.Info("ObjExpExact:");
		foreach (var pair in Config.ObjExpExact) {
			BepLogger.Info("\t" + pair.Key + " -> " + pair.Value);
		}

		BepLogger.Info("ObjExpPrefix:");
		foreach (var tup in Config.ObjExpPrefix) {
			BepLogger.Info("\t" + tup.Prefix + "+ -> " + tup.Expansion);
		}
	}

	public static string GetCommandString(string cmd, List<string> args) {
		string input = cmd;
		if (args.Count > 0)
			input += " " + string.Join(" ", args);
		input = input.ToUpper();
		return input;
	}

	public static bool IsValidColor(string input) {
		var pat = @"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$";
		return Regex.IsMatch(input, pat);
	}

	public static bool IsValidColorWithAlpha(string input) {
		var pat = @"^#([0-9a-fA-F]{8})$";
		return Regex.IsMatch(input, pat);
	}

	public static string RemoveAllNumbers(string input) =>
		System.Text.RegularExpressions.Regex.Replace(input, @"\d", "");

	public static bool GetKeybindDown(KeyCode key, bool shift = false, bool ctrl = false, bool alt = false) {
		return Input.GetKeyDown(key) &&
		       shift == (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) &&
		       ctrl == (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) &&
		       alt == (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt));
	}
}
