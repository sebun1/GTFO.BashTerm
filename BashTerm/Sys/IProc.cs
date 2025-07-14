using BashTerm.Exec;
using BashTerm.Parsers;
using LevelGeneration;

namespace BashTerm.Sys;

public interface IProc {
	static string CommandName { get; }
	static string Desc { get; }
	static string Manual { get; }
	static bool WantDedicatedScreen { get; }
	static FlagSchema FSchema { get; }

	event EventHandler<ExitPayload> OnExit;

	bool Init(UpdatePayload payload, LG_ComputerTerminal term);

	bool Update(UpdatePayload payload);

	PipedPayload Run(string cmd, List<string> args, CmdOpts opts, PipedPayload payload, LG_ComputerTerminal term);
	bool TryGetVarValue(LG_ComputerTerminal term, string varName, out string value);
	bool TryExpandArg(LG_ComputerTerminal term, string arg, out string expanded);
}

