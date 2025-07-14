using BashTerm.Exec;
using BashTerm.Parsers;
using LevelGeneration;

namespace BashTerm.Sys;

public interface IProc {
	/*
	event EventHandler<ExitPayload> OnExit;

	bool Init(UpdatePayload payload, LG_ComputerTerminal term);

	bool Update(UpdatePayload payload);
	*/

	PipedPayload Run(string cmd, List<string> args, CmdOpts opts, PipedPayload payload, LG_ComputerTerminal term);
	bool TryGetVarValue(LG_ComputerTerminal term, string varName, out string value);
	bool TryExpandArg(LG_ComputerTerminal term, string arg, out string expanded);
}

public abstract class ProcBase {
	// TODO: This is not enforced compile time, but it is convention ?
	// should probably enforce some other way
	/*
	 * TODO:
	 * - Not all classes are updated to static files
	 * - Dispatch does not check (but it doesn't really matter because we need to update dispatch anyways)
	 */

	public static string ProcName { get; }
	public static string Desc { get; }
	public static string Manual { get; }
	public static bool WantDedicatedScreen { get; }
	public static FlagSchema FSchema { get; }
}

