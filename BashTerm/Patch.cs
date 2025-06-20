using BashTerm.Exec;
using BashTerm.Parsers;
using Dissonance;
using GameData;
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
				if (TerminalContext.rawMode) {
					__instance.m_terminal.m_command.EvaluateInput(input.ToUpper());
				} else {
					if(ConfigMaster.DEBUG) __instance.m_terminal.AddLine($"{cmd}");
					try {
						Dispatch.Exec(cmd, __instance.m_terminal);
					} catch (Exception e) {
						__instance.m_terminal.m_command.AddOutput(e.Message);
						Logger.Error(e);
					}
					//__instance.m_terminal.m_command.EvaluateInput(commandString(cmd).ToUpper());
				}
				/*
				else if (cmd is LogsCommand) {
					__instance.m_terminal.m_command.EvaluateInput("LOGS");
					__instance.m_terminal.m_command.EvaluateInput("ADMIN_TEMP_OVERRIDE");
					__instance.m_terminal.m_command.EvaluateInput("CLS");
					break;
				} else if (cmd is ListCommand(List<string> args)) {
					LG_ComputerTerminalManager.WantToSendTerminalCommand(__instance.m_terminal.SyncID, TERM_Command.ShowList, "YOU SUCK", "U", "E_494");
					LG_ComputerTerminalManager.WantToSendTerminalCommand(__instance.m_terminal.SyncID, TERM_Command.ShowList, "HAHAHAHA", "U", "E_495");
					break;
				}
				*/
				break;

			case ParseError(string cause):
				__instance.m_terminal.AddLine($"[Parser] error: {cause}");
				break;
		}

		__instance.m_terminal.m_currentLine = ""; // Clear input line
		return false;
	}

	/*
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
	*/

	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.NewLineStart)
	)]
	[HarmonyPostfix]
	public static void NewLineStart(ref LG_ComputerTerminalCommandInterpreter __instance, ref string __result) {
		string res = Styling.Bashterm;

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
		res += Styling.End;
		__result = res;
	}

	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.AddInitialTerminalOutput)
	)]
	[HarmonyPrefix]
	public static bool InitialTerminalPrompt(ref LG_ComputerTerminalCommandInterpreter __instance) {
		// TODO: This doesn't seem to execute on terminals sometimes?
		// You don't see it when you go on a terminal, have to type INFO
		LG_ComputerTerminal term = __instance.m_terminal;
		var localLogs = term.GetLocalLogs();
		int count = localLogs.Count;
		string zone = term.SpawnNode.m_zone.NavInfo.GetFormattedText(LG_NavInfoFormat.Full_And_Number_With_Underscore);
		__instance.AddOutput("---------------------------------------------------------------", spacing: false);
		__instance.AddOutput($"{Styling.Bashterm}BashTerm Shell v{Plugin.BSH_VERSION}{Styling.End}", spacing: false);
		__instance.AddOutput("---------------------------------------------------------------", spacing: false);
		__instance.AddOutput(
			$"Welcome to {Styling.Accent}{term.ItemKey}{Styling.End} located in {Styling.Accent}{zone}{Styling.End}");
		__instance.AddOutput($"There are {count} logs on this terminal", spacing: false);

		if (true) { // TODO: Add to config to enable/disable this?
			foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, TerminalLogFileData> localLog in
			         localLogs) {
				__instance.AddOutput($"{Fmt.Pos(5)}-> {localLog.Key.ToUpper()}{Fmt.EndPos}", spacing: false);
			}
		}

		__instance.AddOutput("", spacing: false);

		__instance.AddOutput("Type \"HELP\" to get help using the terminal.", spacing: false);
		__instance.AddOutput("Type \"COMMANDS\" to get a list of all available commands.", spacing: false);
		__instance.AddOutput("Press [ESC] or type \"EXIT\" to exit");
		return false;
	}

	[HarmonyPatch(
		typeof(LG_TERM_PlayerInteracting),
		nameof(LG_TERM_PlayerInteracting.Enter)
	)]
	[HarmonyPostfix]
	public static void EnterTerminal(ref LG_TERM_PlayerInteracting __instance) {
		TerminalContext.EnterTerminal(__instance.m_terminal);
	}

	[HarmonyPatch(
		typeof(LG_TERM_PlayerInteracting),
		nameof(LG_TERM_PlayerInteracting.Exit)
	)]
	[HarmonyPostfix]
	public static void ExitTerminal() {
		TerminalContext.ExitTerminal();
	}
}
