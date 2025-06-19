using BashTerm.Parsers;
using Dissonance;
using HarmonyLib;
using LevelGeneration;

namespace BashTerm;

[HarmonyPatch]
internal class Patch
{
	[HarmonyPatch(
		typeof(LG_TERM_PlayerInteracting),
		nameof(LG_TERM_PlayerInteracting.OnReturn)
	)]
	[HarmonyPrefix]
	public static bool ProcessCommand (ref LG_TERM_PlayerInteracting __instance)
	{
		var input = __instance.m_terminal.m_currentLine.ToLower();
		var result = MainParser.Parse(input);

		switch (result) {
			case ParsedCommand(Command cmd):
				if (cmd is RawCommand) {
					TerminalContext.toggleRawMode();
					break;
				} else if (cmd is EmptyCommand) {
					__instance.m_terminal.m_command.EvaluateInput("");
					break;
				}

				// TODO: This logic definitely needs to be changed
				if (TerminalContext.rawMode) {
					__instance.m_terminal.m_command.EvaluateInput(input.ToUpper());
				} else {
					__instance.m_terminal.AddLine($"{cmd}");
					__instance.m_terminal.m_command.EvaluateInput(commandString(cmd).ToUpper());
				}

				break;
			case ParseError(string cause):
				__instance.m_terminal.AddLine($"parse error: {cause}");
				break;
		}

		__instance.m_terminal.m_currentLine = ""; // Clear input line

		//LG_ComputerTerminalManager.WantToSendTerminalCommand();

		return false;
	}

	private static string commandString (Command cmd) =>
		cmd switch {
			ListCommand(List<string> args) => $"list {String.Join(' ', args)}",
			QueryCommand(List<string> args) => $"query {String.Join(' ', args)}",
			PingCommand(List<string> args) => $"ping {String.Join(' ', args)}",
			ReactorStartCommand => "reactor_startup",
			ReactorStopCommand => "reactor_shutdown",
			ReactorVerifyCommand(string code) => $"reactor_verify {code}",
			UplinkStartCommand(string address) => $"uplink_connect {address}",
			UplinkVerifyCommand(string code) => $"uplink_verify {code}",
			UplinkConfirmCommand => "uplink_confirm",
			LogsCommand => "logs",
			ReadCommand(string file) => $"read {file}",
			StartCommand(string protocol) => $"start {protocol}",
			InfoCommand => "info",
			HelpCommand => "help",
			ExitCommand => "exit",
			ClearCommand => "cls",
			SpecialCommand(string raw) => raw,
			_ => ""
		};

	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.NewLineStart)
	)]
	[HarmonyPostfix]
	public static void NewLineStart(ref LG_ComputerTerminalCommandInterpreter __instance, ref string __result) {
		string res = "<#C40>";

		string termMode = TerminalContext.rawMode ? "RAW" : "BSH " + Plugin.BSH_VERSION;
		LG_NavInfo zoneNavInfo = __instance.m_terminal.SpawnNode.m_zone.m_navInfo;
		LG_NavInfo areaNavInfo = __instance.m_terminal.SpawnNode.m_area.m_navInfo;
		// __instance.m_terminal.AddLine($"[zoneNavInfo] PrefixLong = {zoneNavInfo.PrefixLong}, PrefixShort = {zoneNavInfo.PrefixShort}, Number = {zoneNavInfo.Number}, UseNumber = {zoneNavInfo.UseNumber}, Suffix = {zoneNavInfo.Suffix}");
		// __instance.m_terminal.AddLine($"[areaNavInfo] PrefixLong = {areaNavInfo.PrefixLong}, PrefixShort = {areaNavInfo.PrefixShort}, Number = {areaNavInfo.Number}, UseNumber = {areaNavInfo.UseNumber}, Suffix = {areaNavInfo.Suffix}");

		res += $"{termMode} ";

		if (__instance.m_terminal.IsPasswordProtected) {
			res += "[PASSWORD]";
		} else {
			res += $"[{zoneNavInfo.Number}{areaNavInfo.Suffix}]";
		}

		res += " >> ";
		res += "</color>";
		__result = res;
	}
}
