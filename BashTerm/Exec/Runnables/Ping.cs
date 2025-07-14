using BashTerm.Parsers;
using BashTerm.Sys;
using LevelGeneration;

namespace BashTerm.Exec.Runnables;

//[CommandHandler("ping")]
public class Ping : ProcBase, IProc {
	public string CommandName => "ping";
	public string Desc => "";
	public static string Manual = new string("");

	public static readonly FlagSchema FSchema = CreateFlagSchema();

	public static FlagSchema CreateFlagSchema() {
		FlagSchema fs = new FlagSchema();
		return fs;
	}

	public PipedPayload Run(string cmd, List<string> args, CmdOpts opts, PipedPayload payload, LG_ComputerTerminal? termInherit) {
		return new EmptyPayload();
	}

	public bool TryGetVarValue(LG_ComputerTerminal term, string varName, out string value) {
		value = "";
		return false;
	}

	public bool TryExpandArg(LG_ComputerTerminal term, string arg, out string expanded) {
		if (ParseUtil.TryExpandObj(arg, out expanded))
			return true;
		expanded = arg;
		return false;
	}
}
