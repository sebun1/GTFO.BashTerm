using BashTerm.Parsers;
using BashTerm.Sys;
using LevelGeneration;

namespace BashTerm.Exec.Runnables;

[BshProc("raw")]
public class Raw : IProc {
	public string CommandName => "raw";
	public string Desc => "Toggle between BashTerm interpreter and raw input (GTFO native interpreter)";
	public string Manual => "Use this command to switch to GTFO native interpreter when BashTerm misbehaves, feel free to report any problems or bugs!";

	public static readonly FlagSchema FSchema = CreateFlagSchema();

	public static FlagSchema CreateFlagSchema() {
		FlagSchema fs = new FlagSchema();
		return fs;
	}

	public PipedPayload Run(string cmd, List<string> args, CmdOpts opts, PipedPayload payload, LG_ComputerTerminal terminal) {
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);
		BshSystem.ToggleRawMode();
		terminal.m_command.AddOutput("", spacing: false);
		return new EmptyPayload();
	}

	public bool TryGetVarValue(LG_ComputerTerminal term, string varName, out string value) {
		value = "";
		return false;
	}

	public bool TryExpandArg(LG_ComputerTerminal term, string arg, out string expanded) {
		expanded = "";
		return false;
	}
}
