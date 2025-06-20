using System.Collections.Generic;
using Il2CppSystem.Xml.Schema;

namespace BashTerm;

internal static class Util {
	// All methods assume and should have calls with lower case strings

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

	public static bool ContainsFlag(params string[] args) {
		foreach (string arg in args)
			if (arg.StartsWith("-"))
				return true;
		return false;
	}

	public static bool IsInt(string input) {
		return int.TryParse(input, out _);
	}

	public static void printMaps() {
		Logger.Info("CmdExpExact:");
		foreach (var pair in ConfigMaster.CmdExpExact) {
			Logger.Info("\t" + pair.Key + " -> " + pair.Value);
		}
		Logger.Info("CmdExpPrefix:");
		foreach (var tup in ConfigMaster.CmdExpPrefix) {
			Logger.Info("\t" + tup.Prefix + "+ -> " + tup.Expansion);
		}
		Logger.Info("ObjExpExact:");
		foreach (var pair in ConfigMaster.ObjExpExact) {
			Logger.Info("\t" + pair.Key + " -> " + pair.Value);
		}
		Logger.Info("ObjExpPrefix:");
		foreach (var tup in ConfigMaster.ObjExpPrefix) {
			Logger.Info("\t" + tup.Prefix + "+ -> " + tup.Expansion);
		}
	}

	public static string GetCommandString(string cmd, List<string> args) {
		string input = cmd;
		if (args.Count > 0)
			input += " " + string.Join(" ", args);
		input = input.ToUpper();
		return input;
	}
}
