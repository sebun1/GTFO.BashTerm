using LevelGeneration;

namespace BashTerm.Exec.Runnables;

[CommandHandler("raw")]
public class Raw : IRunnable {
	public string CommandName => "raw";
	public string Desc => "Toggle between BashTerm interpreter and raw input (GTFO native interpreter)";
	public string Man => "Use this command to switch to GTFO native interpreter when BashTerm misinterprets, please also report any problems with the interpreter!";

	public PipedPayload Run(string cmd, List<string> args, PipedPayload payload, LG_ComputerTerminal terminal) {
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);
		TerminalChan.ToggleRawMode();
		terminal.m_command.AddOutput("", spacing: false);
		return new EmptyPayload();
	}
}
