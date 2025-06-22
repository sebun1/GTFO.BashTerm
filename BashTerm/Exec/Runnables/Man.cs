using LevelGeneration;

namespace BashTerm.Exec.Runnables;

[CommandHandler("man")]
public class Man : IRunnable {
	public string CommandName => "man";
	public string Desc => "Queries the location of a single item (or multiple through piping)";

	public string Manual => @"
NAME
		man - read the manual for a given command

SYNOPSIS
		MAN <u>COMMAND</u>
";

	public PipedPayload Run(string cmd, List<string> args, PipedPayload payload, LG_ComputerTerminal terminal) {
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);

		if (!Dispatch.IsInitialized) {
			terminal.m_command.AddOutput("");
			TerminalChan.LogError("Man", "tried to get manual before Dispatch is initialized, this is impossible??");
			throw new CmdRunException("manuals aren't loaded yet");
		}

		if (args.Count < 1) throw new MissingArgumentException(CommandName, args.Count, 1);
		if (args.Count > 1) throw new TooManyArgumentsException(CommandName, args.Count, 1);

		if (Dispatch.Handlers.TryGetValue(args[0], out var runnable)) {
			terminal.m_command.AddOutput($"Showing manual for [{cmd}]:", spacing: false);
			// TODO: The output is weird with indentation and line wrapping
			terminal.m_command.AddOutput(Util.ReplaceTabWithSpaces(runnable.Manual));
		}

		return new EmptyPayload();
	}
}
