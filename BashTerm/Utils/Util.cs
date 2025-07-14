using System.Collections.Generic;
using Il2CppSystem.Xml.Schema;

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
		Logger.Info("CmdExpExact:");
		foreach (var pair in ConfigMgr.CmdExpExact) {
			Logger.Info("\t" + pair.Key + " -> " + pair.Value);
		}
		Logger.Info("CmdExpPrefix:");
		foreach (var tup in ConfigMgr.CmdExpPrefix) {
			Logger.Info("\t" + tup.Prefix + "+ -> " + tup.Expansion);
		}
		Logger.Info("ObjExpExact:");
		foreach (var pair in ConfigMgr.ObjExpExact) {
			Logger.Info("\t" + pair.Key + " -> " + pair.Value);
		}
		Logger.Info("ObjExpPrefix:");
		foreach (var tup in ConfigMgr.ObjExpPrefix) {
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

	/// <summary>
	/// Computes the effective display width excluding xml-like TextMeshPro tags
	/// </summary>
	/// <param name="txt">Text to be analyzed</param>
	/// <returns>Effective width of the line given a single line, -1 otherwise</returns>
	public static int GetRealLineWidth(string txt) {
		// TODO
		return -1;
	}

	public static string RemoveAllNumbers(string input) =>
		System.Text.RegularExpressions.Regex.Replace(input, @"\d", "");
}
