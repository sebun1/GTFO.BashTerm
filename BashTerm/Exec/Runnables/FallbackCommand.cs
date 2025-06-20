using LevelGeneration;

namespace BashTerm.Exec.Runnables;

public class FallbackCommand : IRunnable {
	public string CommandName => "FALLBACK";
	public string Desc => "Fallback handler for simple, special, or unrecognized commands.";
	public string Man => "Executes commands through the GTFO interpreter, should never be called manually";

	public PipedPayload Run(string cmd, List<string> args, PipedPayload payload, LG_ComputerTerminal terminal) {
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);

		terminal.m_command.EvaluateInput(String.Join(' ', new[]{ cmd }.Concat(args)).ToUpper());
		return new EmptyPayload();
	}
}
