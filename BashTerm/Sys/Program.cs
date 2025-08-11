using BashTerm.Exec;
using BashTerm.Parsers;
using LevelGeneration;

namespace BashTerm.Sys;

public class ProcessContext {
	public LG_ComputerTerminal Terminal { get; private set; }
	public List<string> Args { get; private set; }
	public CmdOpts Opts { get; private set; }
	public PipeStream StdIn { get; private set; }
	public PipeStream StdOut { get; private set; }
	public PipeStream StdErr { get; private set; }

	public ProcessContext(LG_ComputerTerminal term, List<string> args, CmdOpts opts,
		PipeStream stdIn, PipeStream stdOut, PipeStream stdErr) {
		Terminal = term;
		StdIn = stdIn;
		StdOut = stdOut;
		StdErr = stdErr;
	}
}

/// <summary>
///
/// <c>Proc</c> - Defines a base process for all Bsh processes.
/// <para>
/// All Proc children should implement a
///	<code>public static ProcManifest GetManifest()</code>
/// function to be registered with BshSystem at startup. If
/// unspecified, the process will not be registered and not callable
/// </para>
/// </summary>
public abstract class Program {
	public event EventHandler<ExitPayload>? OnExit;

	protected virtual void RaiseOnExit(ExitPayload payload) {
		OnExit?.Invoke(this, payload);
	}
	public abstract void Start(ProcessContext context);
	public abstract void Update();

	public virtual void OnSigInt() {
		OnExit?.Invoke(this, null);
	}

	public void OnSigQuit() {
		OnExit?.Invoke(this, null);
	}

	public virtual void OnSigStp() {}
}

public class ProgramManifest {
	public string ProgramName;
	public string Desc;
	public string Manual;
	public bool WantDiscreteOutputBuffer;
	public FlagSchema FSchema;

	public ProgramManifest(string name, string desc, string manual, bool wantScreen, FlagSchema fSchema) {
		ProgramName = name;
		Desc = desc;
		Manual = manual;
		WantDiscreteOutputBuffer = wantScreen;
		FSchema = fSchema;
	}
}
