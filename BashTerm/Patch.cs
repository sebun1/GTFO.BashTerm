using BashTerm.Parsers;
using BashTerm.Exec;
using BashTerm.Sys;
using BashTerm.Utils;
using GameData;
using HarmonyLib;
using LevelGeneration;
using TenCC.Utils;
using UnityEngine;
using UnityEngine.UI;

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

		__instance.m_terminal.m_text.Rebuild(CanvasUpdate.PreRender);

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

			if (BshSystem.UserRawMode) {
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
			__instance.m_terminal.m_command.AddOutput($"{Styles.C_Error}{e}{Styles.C_End}");
			Log.Error(e);
		}
		catch (Exception e) {
			__instance.m_terminal.m_command.AddOutput($"{Styles.C_Error}{e}{Styles.C_End}");
			Log.Error(e);
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
		string res = Styles.C_Bsh;

		string termMode = BshSystem.UserRawMode ? "RAW" : "BSH " + Plugin.BSH_VERSION;
		LG_NavInfo zoneNavInfo = __instance.m_terminal.SpawnNode.m_zone.m_navInfo;
		LG_NavInfo areaNavInfo = __instance.m_terminal.SpawnNode.m_area.m_navInfo;

		res += $"{termMode} ";

		if (__instance.m_terminal.IsPasswordProtected) {
			res += "[PASSWORD]";
		} else {
			res += $"[{zoneNavInfo.Number}{areaNavInfo.Suffix}]";
		}

		res += " >> ";
		res += Styles.C_End;
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
		Log.Debug($"ReceiveCmd with cmd={cmd} inputLine={inputLine}");
		if (Sync.Signal(new SyncSrcOnReceiveCmd(__instance.m_terminal.m_serialNumber))) {
			// TODO: correspond with managing in
			Log.Debug("Successfully signaled SyncSrcOnReceiveCmd");
		} else {
			Log.Debug("There was nothing to signal for SyncSrcOnReceiveCmd");
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
		string playerName = __instance.m_terminal.m_localInteractionSource?.PlayerName;
		__instance.AddOutput("---------------------------------------------------------------", spacing: false);
		__instance.AddOutput($"{Styles.C_Bsh}BSH v{Plugin.BSH_VERSION}{Styles.C_End}", spacing: false);
		__instance.AddOutput("---------------------------------------------------------------", spacing: false);
		__instance.AddOutput($"Hi {Styles.C_Bsh}{playerName}{Styles.C_End}, welcome back!");
		__instance.AddOutput( $"Welcome to {Styles.C_Accent}{term.ItemKey}{Styles.C_End} located in {Styles.C_Accent}{zone}{Styles.C_End}");
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
		nameof(LG_TERM_PlayerInteracting.ParseInput)
	)]
	[HarmonyPrefix]
	public static bool ParseInput(ref LG_TERM_PlayerInteracting __instance) {
		if (BshSystem.UserRawMode) {
			return true;
		}

		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			// history prev
			Log.Debug("KEYDOWN: UpArrow");
		} else if (Input.GetKeyDown(KeyCode.DownArrow)) {
			// history next
			Log.Debug("KEYDOWN: DownArrow");
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			// cursor left
			Log.Debug("KEYDOWN: LeftArrow");
		} else if (Input.GetKeyDown(KeyCode.RightArrow)) {
			// cursor right
			Log.Debug("KEYDOWN: RightArrow");
		}

		if (!string.IsNullOrEmpty(Input.inputString))
			Log.Debug($"Input.inputString={Input.inputString}");

		return true;
		return false;
	}

	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.UpdateTerminalScreen)
	)]
	[HarmonyPrefix]
	public static bool UpdateTerminalScreen(ref LG_ComputerTerminalCommandInterpreter __instance, string currentLine,
		bool hasLocalPlayer) {
		// TODO: Overwrite entire UpdateTerminalScreen logic
		// (things like press any key to continue every max_line is not necessary etc.)
		// CharBuffer t = new CharBuffer();
		//__instance.m_text.SetCharArray();
		//__instance.m_text.text = "Test";

		//return false;
		return true;
	}

	[HarmonyPatch(
		typeof(LG_TERM_PlayerInteracting),
		nameof(LG_TERM_PlayerInteracting.Enter)
	)]
	[HarmonyPostfix]
	public static void EnterTerminal(ref LG_TERM_PlayerInteracting __instance) {
		Log.Debug($"Entered Terminal syncID={__instance.m_terminal.SyncID}, serialNumber={__instance.m_terminal.m_serialNumber}");
	}

	[HarmonyPatch(
		typeof(LG_TERM_PlayerInteracting),
		nameof(LG_TERM_PlayerInteracting.Exit)
	)]
	[HarmonyPostfix]
	public static void ExitTerminal(ref LG_TERM_PlayerInteracting __instance) {
		Log.Debug($"Exit Terminal syncID={__instance.m_terminal.SyncID}, serialNumber={__instance.m_terminal.m_serialNumber}");
	}
}
