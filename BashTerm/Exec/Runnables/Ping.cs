using LevelGeneration;

namespace BashTerm.Exec.Runnables;

//[CommandHandler("ping")]
public class Ping : IRunnable {
	public string CommandName => "ping";
	public string Desc => "";
	public string Man => "";

	public PipedPayload Run(string cmd, List<string> args, PipedPayload payload, LG_ComputerTerminal? termInherit) {
		return new EmptyPayload();
	}
}
