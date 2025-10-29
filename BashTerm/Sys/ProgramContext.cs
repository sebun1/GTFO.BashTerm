using BashTerm.Exec;
using BashTerm.Parsers;
using BashTerm.Sys.Stream;

namespace BashTerm.Sys;

public class ProgramContext {
	public Terminal Terminal { get; private set; }
	public List<string> Args { get; private set; }
	public CmdOpts Opts { get; private set; }

	public PipeStreamReader<byte> StdIn { get; private set; }

	public PipeStreamReader<PipeObject> ObjIn { get; private set; }

	public PipeStreamWriter<byte> StdOut { get; private set; }

	public PipeStreamWriter<PipeObject> ObjOut { get; private set; }

	public ProgramContext(Terminal term,
		List<string> args, CmdOpts opts,
		PipeStreamReader<byte> stdIn, PipeStreamReader<PipeObject> objIn,
		PipeStreamWriter<byte> stdOut, PipeStreamWriter<PipeObject> objOut) {
		Terminal = term;
		Args = args;
		Opts = opts;
		StdIn = stdIn;
		ObjIn = objIn;
		StdOut = stdOut;
		ObjOut = objOut;
	}
}
