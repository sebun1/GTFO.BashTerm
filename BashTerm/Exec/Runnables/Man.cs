using System.Reflection;
using BashTerm.Parsers;
using BashTerm.Sys;
using BashTerm.Utils;
using LevelGeneration;

namespace BashTerm.Exec.Runnables;

[BshProc("man")]
public class Man : IProc {
	public string CommandName => "man";
	public string Desc => "Read the manual for a given command";

	public string Manual => @"
NAME
		man - read the manual for a given command

USAGE
		MAN <u>COMMAND</u>
";

	public static readonly FlagSchema FSchema = CreateFlagSchema();

	public static FlagSchema CreateFlagSchema() {
		FlagSchema fs = new FlagSchema();
		return fs;
	}

	public PipedPayload Run(string cmd, List<string> args, CmdOpts opts, PipedPayload payload, LG_ComputerTerminal terminal) {
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);

		if (!Dispatch.IsInitialized) {
			terminal.m_command.AddOutput("");
			Bsh.LogError("Man", "tried to get manual before Dispatch is initialized, this is impossible??");
			throw new CmdRunException("manuals aren't loaded yet");
		}

		if (args.Count < 1) throw new MissingArgumentException(CommandName, args.Count, 1);
		if (args.Count > 1) throw new TooManyArgumentsException(CommandName, args.Count, 1);

		if (Dispatch.HandlerTypes.TryGetValue(args[0], out Type procType)) {
			FieldInfo manualField = procType.GetField("Manual", BindingFlags.Public | BindingFlags.Static);
			string man = (string)manualField.GetValue(null);
			terminal.m_command.AddOutput($"Showing manual for [{cmd}]:", spacing: false);
			terminal.m_command.AddOutput(Fmt.Wrap(man));
		}

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
