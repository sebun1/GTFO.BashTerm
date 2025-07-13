using BashTerm.Parsers;
using LevelGeneration;

namespace BashTerm.Exec.Runnables;

//[CommandHandler("ping")]
public class Ping : IRunnable {
	public string CommandName => "ping";
	public string Desc => "";
	public string Manual => "";

	public FlagSchema FSchema { get; }

	public Ping() {
		FSchema = new FlagSchema();
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
