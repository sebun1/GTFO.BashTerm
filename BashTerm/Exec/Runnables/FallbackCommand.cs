using BashTerm.Parsers;
using LevelGeneration;

namespace BashTerm.Exec.Runnables;

public class FallbackCommand : IRunnable {
	public string CommandName => "FALLBACK";
	public string Desc => "Fallback handler for simple, special, or unrecognized commands.";
	public string Manual => "Executes commands through the GTFO interpreter, should never be called manually";

	public FlagSchema FSchema { get; }

	public FallbackCommand() {
		FSchema = new FlagSchema();
	}

	public async Task<PipedPayload> Run(string cmd, List<string> args, CmdOpts opts, PipedPayload payload, LG_ComputerTerminal terminal) {
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);

		terminal.m_command.EvaluateInput(String.Join(' ', new[]{ cmd }.Concat(args)).ToUpper());
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
