using Bsh.Utils;
using Bsh.Sys;
using HarmonyLib;
using LevelGeneration;

namespace Bsh.Patches;

[HarmonyPatch]
public class ScreenPatch {
	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.AddOutput),
		new[] { typeof(List<string>) }
	)]
	[HarmonyPrefix]
	public static bool AddOutputList(ref LG_ComputerTerminalCommandInterpreter __instance, List<string> lines) {
		if (__instance.m_terminal?.m_command?.m_text == null) return true;

		BshLogger.Debug($"[AddOutput List]: Count={lines.Count}");
		foreach (string line in lines) {
			BshLogger.Debug($"LINE >> \"{line}\"");
		}

		return true;
	}

	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.AddOutput),
		new[] {
			typeof(TerminalLineType), typeof(string), typeof(float),
			typeof(TerminalSoundType), typeof(TerminalSoundType)
		}
	)]
	[HarmonyPrefix]
	public static bool AddOutputLineAdvanced(ref LG_ComputerTerminalCommandInterpreter __instance,
		TerminalLineType type, string line, float time,
		TerminalSoundType onPrintSound, TerminalSoundType onWaitDoneSound) {
		if (__instance.m_terminal?.m_command?.m_text == null) return true;

		BshLogger.Debug($"[AddOutput Advanced]: \"{line}\", time={time}");

		return true;
	}

	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.AddOutput),
		new[] { typeof(string), typeof(bool) }
	)]
	[HarmonyPrefix]
	public static bool AddOutputLine(ref LG_ComputerTerminalCommandInterpreter __instance,
		string originalLine, bool spacing) {
		if (__instance.m_terminal?.m_command?.m_text == null) return true;

		BshLogger.Debug($"[AddOutput Line]: \"{originalLine}\"");

		return true;
	}
}
