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

public class ProcManifest {
	public static string ProcName;
	public static string Desc;
	public static string Manual;
	public static bool WantDedicatedScreen;
	public static FlagSchema FSchema;

	public ProcManifest(string name, string desc, string manual, bool wantScreen, FlagSchema fSchema) {
		ProcName = name;
		Desc = desc;
		Manual = manual;
		WantDedicatedScreen = wantScreen;
		FSchema = fSchema;
	}
}
