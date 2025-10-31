using Bsh.Parsers;
using Bsh.Sys;
using LevelGeneration;

namespace Bsh.Exec.Programs;

[BshProgram("raw")]
public class Raw : Program {
	public string CommandName => "raw";
	public string Desc => "Toggle between BashTerm interpreter and raw input (GTFO native interpreter)";

	public string Manual =>
		"Use this command to switch to GTFO native interpreter when BashTerm misbehaves, feel free to report any problems or bugs!";

	public static readonly FlagSchema FSchema = CreateFlagSchema();

	public static FlagSchema CreateFlagSchema() {
		FlagSchema fs = new FlagSchema();
		return fs;
	}

	public PipeObject Run(string cmd, List<string> args, CmdOpts opts, PipeObject payload,
		LG_ComputerTerminal terminal) {
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);
		BshSystem.ToggleRawMode();
		terminal.m_command.AddOutput("", spacing: false);
		return new NullObject();
	}

	public bool TryGetVarValue(LG_ComputerTerminal term, string varName, out string value) {
		value = "";
		return false;
	}

	public bool TryExpandArg(LG_ComputerTerminal term, string arg, out string expanded) {
		expanded = "";
		return false;
	}

	public override void Start() {
		throw new NotImplementedException();
	}

	public override void Update() {
		throw new NotImplementedException();
	}
}
