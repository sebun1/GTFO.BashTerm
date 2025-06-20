using LevelGeneration;

namespace BashTerm.Exec.Runnables;

public class FallbackCommand : IRunnable {
	public string CommandName => "FALLBACK";
	public string Description => "Fallback handler for simple, special, or unrecognized commands.";
	public string Help => "Executes commands through the GTFO interpreter, should never be called manually";

	public PipedPayload Run(string cmd, List<string> args, PipedPayload payload, LG_ComputerTerminal? termInherit) {
		var terminal = termInherit == null ? TerminalContext.TerminalInstance : termInherit;
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);
		TerminalContext.EvaluateInput(String.Join(' ', new[]{ cmd }.Concat(args)).ToUpper());
		return new EmptyPayload();
	}
}
