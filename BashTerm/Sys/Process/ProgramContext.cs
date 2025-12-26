using Bsh.Exec;
using Bsh.Parsers;
using Bsh.Sys.Render;
using Bsh.Sys.Stream;

namespace Bsh.Sys.Process;

public class ProgramContext {
	public int Pid { get; set; }
	public Terminal Terminal { get; private set; }
	public List<string> Args { get; private set; }
	public CmdOpts Opts { get; private set; }

	// These should have a capacity of 16384 (16 KiB) by default
	public PipeStreamReader<byte> StdIn { get; private set; }

	public PipeStreamReader<PipeObject> ObjIn { get; private set; }

	public PipeStreamWriter<byte> StdOut { get; private set; }

	public PipeStreamWriter<PipeObject> ObjOut { get; private set; }

	public readonly bool HasPane;
	public readonly Pane? Pane;

	public ProgramContext(int pid, Terminal term,
		List<string> args, CmdOpts opts,
		Pane? pane,
		PipeStreamReader<byte> stdIn, PipeStreamReader<PipeObject> objIn,
		PipeStreamWriter<byte> stdOut, PipeStreamWriter<PipeObject> objOut) {
		Pid = pid;
		Terminal = term;
		Args = args;
		Opts = opts;
		HasPane = pane != null;
		Pane = pane;
		StdIn = stdIn;
		ObjIn = objIn;
		StdOut = stdOut;
		ObjOut = objOut;
	}
}
