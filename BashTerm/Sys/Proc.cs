using BashTerm.Exec;
using BashTerm.Parsers;
using LevelGeneration;

namespace BashTerm.Sys;

/// <summary>
///
/// <c>Proc</c> - Defines a base process for all Bsh processes.
/// <para>
/// All Proc children should implement a
///	<code>public static ProcManifest GetManifest()</code>
/// function to be registered with BshSystem at startup. If
/// unspecified, the process will not be register and not callable
/// </para>
/// </summary>
public abstract class Proc {
	public event EventHandler<ExitPayload>? OnExit;

	protected virtual void RaiseOnExit(ExitPayload payload) {
		OnExit?.Invoke(this, payload);
	}
	public abstract void Start(StartPayload payload, LG_ComputerTerminal term);
	public abstract void Update(UpdatePayload payload);

	public virtual void OnSigInt() {
		OnExit?.Invoke(this, null);
	}

	public virtual void OnSigQuit() {
		OnExit?.Invoke(this, null);
	}

	public virtual void OnSigStp() {}
}

public class ProcManifest {
	public string ProcName;
	public string Desc;
	public string Manual;
	public bool WantDiscreteOutputBuffer;
	public FlagSchema FSchema;

	public ProcManifest(string name, string desc, string manual, bool wantScreen, FlagSchema fSchema) {
		ProcName = name;
		Desc = desc;
		Manual = manual;
		WantDiscreteOutputBuffer = wantScreen;
		FSchema = fSchema;
	}
}
