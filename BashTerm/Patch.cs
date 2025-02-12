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
				__instance.m_terminal.AddLine($"{cmd}");
				__instance.m_terminal.m_command.EvaluateInput(commandString(cmd));
				break;
			case ParseError(string cause):
				__instance.m_terminal.AddLine($"parse error: {cause}");
				break;
		}

		return false;
	}

	private static string commandString (Command cmd) =>
		cmd switch {
			ListCommand(List<string> args) => $"list {String.Join(' ', args)}",
			QueryCommand(string arg) => $"query {arg}",
			PingCommand(string arg) => $"ping {arg}"
		};
}
