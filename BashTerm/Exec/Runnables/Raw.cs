using LevelGeneration;

namespace BashTerm.Exec.Runnables;

[CommandHandler("raw")]
public class Raw : IRunnable {
	public string CommandName => "raw";
	public string Description => "Toggle between BashTerm interpreter and raw input (GTFO native interpreter)";
	public string Help => "Use this command to switch to GTFO native interpreter when BashTerm misinterprets, please also report any problems with the interpreter!";

	public PipedPayload Run(string cmd, List<string> args, PipedPayload payload, LG_ComputerTerminal? termInherit) {
		var terminal = termInherit == null ? TerminalContext.TerminalInstance : termInherit;
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);
		TerminalContext.ToggleRawMode();
		terminal.m_command.AddOutput("", spacing: false);
		return new EmptyPayload();
	}
}
