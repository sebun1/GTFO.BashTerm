using BashTerm.Parsers;
using LevelGeneration;

namespace BashTerm.Sys;

public class ProgramContext {
	public LG_ComputerTerminal Terminal { get; private set; }
	public List<string> Args { get; private set; }
	public CmdOpts Opts { get; private set; }
	public PipeStream StdIn { get; private set; }
	public PipeStream StdOut { get; private set; }
	public PipeStream StdErr { get; private set; }

	public ProgramContext(LG_ComputerTerminal term, List<string> args, CmdOpts opts,
		PipeStream stdIn, PipeStream stdOut, PipeStream stdErr) {
		Terminal = term;
		Args = args;
		Opts = opts;
		StdIn = stdIn;
		StdOut = stdOut;
		StdErr = stdErr;
	}
}
