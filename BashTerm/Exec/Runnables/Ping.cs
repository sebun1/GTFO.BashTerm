using LevelGeneration;

namespace BashTerm.Exec.Runnables;

//[CommandHandler("ping")]
public class Ping : IRunnable {
	public string CommandName => "ping";
	public string Desc => "";
	public string Manual => "";

	public PipedPayload Run(string cmd, List<string> args, PipedPayload payload, LG_ComputerTerminal? termInherit) {
		return new EmptyPayload();
	}

	public bool TryGetVar(LG_ComputerTerminal term, string varName, out string value) {
		value = "";
		return false;
	}

	public bool TryExpandArg(LG_ComputerTerminal term, string arg, out string expanded) {
		expanded = "";
		return false;
	}
}
