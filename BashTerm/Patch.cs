using BashTerm.Parsers;
using BashTerm.Utils;
using BashTerm.Exec;
using GameData;
using HarmonyLib;
using LevelGeneration;

namespace BashTerm;

[HarmonyPatch]
internal class Patch {
	[HarmonyPatch(
		typeof(LG_TERM_PlayerInteracting),
		nameof(LG_TERM_PlayerInteracting.OnReturn)
	)]
	[HarmonyPrefix]
	public static bool ProcessCommand(ref LG_TERM_PlayerInteracting __instance) {
		Bsh.Renew(__instance.m_terminal);
		// TODO: Consider not making it lower, allow case variation. Will want to change other code that returns uppercase, if any
		var input = __instance.m_terminal.m_currentLine.ToLower();

		try {
			/*
				m_inputBuffer.Add(inputString);
				m_terminal.m_caretBlinkOffsetFromEnd = 0;
				m_inputBufferStep = 0;
				if (m_inputBuffer.Count > 10)
				{
					m_inputBuffer.RemoveAt(0);
				}
			 */

			if (BshSystem.RawMode) {
				if (input.Trim().ToLower().Split(' ')[0] == "raw") {
					BshSystem.ToggleRawMode();
				} else {
					__instance.m_terminal.m_command.EvaluateInput(input.ToUpper());
				}
			} else {
				VarCommand cmd = MainParser.Parse(input);
				Dispatch.Exec(cmd, __instance.m_terminal);
				//if (ConfigMgr.DEBUG) __instance.m_terminal.m_command.AddOutput($"{Styles.Info}{cmd.FmtToString()}{Styles.CEnd}");
				if (ConfigMgr.DEBUG) __instance.m_terminal.m_command.AddOutput($"{cmd.FmtToString()}");
			}
		}
		catch (BSHException e) {
			__instance.m_terminal.m_command.AddOutput($"{Styles.Error}{e}{Styles.CEnd}");
			Logger.Error(e);
		}
		catch (Exception e) {
			__instance.m_terminal.m_command.AddOutput($"{Styles.Error}{e}{Styles.CEnd}");
			Logger.Error(e);
		}

		__instance.m_terminal.m_currentLine = "";
		Bsh.Expire();
		return false;
	}

	/*
	[HarmonyPatch(
		typeof(LG_TERM_PlayerInteracting),
		nameof(LG_TERM_PlayerInteracting.ParseInput)
	)]
	[HarmonyPrefix]
	public static bool ParseInput(ref LG_TERM_PlayerInteracting __instance) {
		// TODO

		InputMapper.GetButtonDownKeyMouseGamepad(InputAction.TerminalDel, eFocusState.ComputerTerminal);

		// Input.inputString =

		return false;
	}
	*/

	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.NewLineStart)
	)]
	[HarmonyPostfix]
	public static void NewLineStart(ref LG_ComputerTerminalCommandInterpreter __instance, ref string __result) {
		string res = Styles.Bashterm;

		string termMode = BshSystem.RawMode ? "RAW" : "BSH " + Plugin.BSH_VERSION;
		LG_NavInfo zoneNavInfo = __instance.m_terminal.SpawnNode.m_zone.m_navInfo;
		LG_NavInfo areaNavInfo = __instance.m_terminal.SpawnNode.m_area.m_navInfo;

		res += $"{termMode} ";

		if (__instance.m_terminal.IsPasswordProtected) {
			res += "[PASSWORD]";
		} else {
			res += $"[{zoneNavInfo.Number}{areaNavInfo.Suffix}]";
		}

		res += " >> ";
		res += Styles.CEnd;
		__result = res;
	}

	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.ReceiveCommand)
	)]
	[HarmonyPrefix]
	public static bool ReceiveCmdPre(ref LG_ComputerTerminalCommandInterpreter __instance, TERM_Command cmd,
		string inputLine) {
		if (cmd == TERM_Command.EmptyLine && Bsh.DetectSyncedIO(inputLine))
			return false;
		return true;
	}

	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.ReceiveCommand)
	)]
	[HarmonyPostfix]
	public static void ReceiveCmd(ref LG_ComputerTerminalCommandInterpreter __instance, TERM_Command cmd, string inputLine) {
		Logger.Debug($"ReceiveCmd with cmd={cmd} inputLine={inputLine}");
		if (Sync.Signal(new SyncSrcOnReceiveCmd(__instance.m_terminal.m_serialNumber))) {
			// TODO: correspond with managing in
			Logger.Debug("Successfully signaled SyncSrcOnReceiveCmd");
		} else {
			Logger.Debug("There was nothing to signal for SyncSrcOnReceiveCmd");
		}
	}

	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.AddInitialTerminalOutput)
	)]
	[HarmonyPrefix]
	public static bool InfoPrompt(ref LG_ComputerTerminalCommandInterpreter __instance) {
		// TODO: This doesn't seem to execute on terminals sometimes?
		// You don't see it when you go on a terminal, have to type INFO
		LG_ComputerTerminal term = __instance.m_terminal;
		var localLogs = term.GetLocalLogs();
		int count = localLogs.Count;
		string zone = term.SpawnNode.m_zone.NavInfo.GetFormattedText(LG_NavInfoFormat.Full_And_Number_With_Underscore);
		__instance.AddOutput("---------------------------------------------------------------", spacing: false);
		__instance.AddOutput($"{Styles.Bashterm}BashTerm Shell v{Plugin.BSH_VERSION}{Styles.CEnd}", spacing: false);
		__instance.AddOutput("---------------------------------------------------------------", spacing: false);
		__instance.AddOutput(
			$"Welcome to {Styles.Accent}{term.ItemKey}{Styles.CEnd} located in {Styles.Accent}{zone}{Styles.CEnd}");
		string isOrAre = count > 1 ? "are" : "is";
		string sOrNoS = count > 1 ? "s" : "";
		__instance.AddOutput($"There {isOrAre} {count} log{sOrNoS} on this terminal", spacing: false);

		if (true) {
			// TODO: Add to config to enable/disable this?
			foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, TerminalLogFileData> localLog in
			         localLogs) {
				__instance.AddOutput($"{Styles.Pos(5)}-> {localLog.Key.ToUpper()}{Styles.EndPos}", spacing: false);
			}
		}

		__instance.AddOutput("", spacing: false);

		__instance.AddOutput("Type \"HELP\" to get help using the terminal.", spacing: false);
		__instance.AddOutput("Type \"COMMANDS\" to get a list of all available commands.", spacing: false);
		__instance.AddOutput("Type \"HELP BSH\" to get help using BashTerm", spacing: false);
		__instance.AddOutput("Type \"MAN <COMMAND>\" to read the manual for a command", spacing: false);
		__instance.AddOutput("Press [ESC] or type \"EXIT\" to exit");
		return false;
	}

	[HarmonyPatch(
		typeof(LG_TERM_PlayerInteracting),
		nameof(LG_TERM_PlayerInteracting.Enter)
	)]
	[HarmonyPostfix]
	public static void EnterTerminal(ref LG_TERM_PlayerInteracting __instance) {
	}

	[HarmonyPatch(
		typeof(LG_TERM_PlayerInteracting),
		nameof(LG_TERM_PlayerInteracting.Exit)
	)]
	[HarmonyPostfix]
	public static void ExitTerminal() {
	}
}
