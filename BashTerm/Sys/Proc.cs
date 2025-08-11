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
/// unspecified, the process will not be registered/callable
/// </para>
/// </summary>
public abstract class Proc {
	public event EventHandler<ExitPayload>? OnExit;

	protected void Exit(ExitPayload payload) {
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
	public string Name;
	public string Desc;
	public string Manual;
	public bool RequestAlternateBuffer;
	public FlagSchema FSchema;

	public ProcManifest(string name, string desc, string manual, bool wantBuffer, FlagSchema fSchema) {
		Name = name;
		Desc = desc;
		Manual = manual;
		RequestAlternateBuffer = wantBuffer;
		FSchema = fSchema;
	}
}
