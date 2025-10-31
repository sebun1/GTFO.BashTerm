using Bsh.Parsers;

namespace Bsh.Sys;

public enum eProgramState {
	Active,
	Inactive,
	Exited
}

/// <summary>
///
/// <c>Program</c> - Defines a base program structure for all Bsh processes.
/// <para>
/// All Proc children should implement a
///	<code>public static ProgramManifest GetManifest()</code>
/// function to be registered with BshSystem at startup. If
/// unspecified, the process will not be registered and not callable
/// </para>
/// </summary>
public abstract class Program {
	private bool _initialized = false;
	protected ProgramContext? Ctx;
	public event EventHandler? OnExit;
	public eProgramState State { get; private set; } = eProgramState.Inactive;

	internal void SetActive() {
		State = eProgramState.Active;
	}

	internal void SetInactive() {
		State = eProgramState.Inactive;
	}

	internal void Init(ProgramContext ctx) {
		if (_initialized)
			throw new BshSystemException("Program already initialized");
		Ctx = ctx;
		OnExit += (_, _) => { State = eProgramState.Exited; };
		_initialized = true;
	}

	protected void Exit() {
		OnExit?.Invoke(this, EventArgs.Empty);
	}

	public abstract void Start();
	public abstract void Update();

	public virtual void OnSigInt() {
		OnExit?.Invoke(this, null);
	}

	public void OnSigQuit() {
		OnExit?.Invoke(this, null);
	}

	public virtual void OnSigStp() {
	}
}

public class ProgramManifest {
	public string ProgramName;
	public string Description;
	public string Manual;
	public bool WantDiscreteOutputBuffer;
	public FlagSchema FSchema;

	public ProgramManifest(string name, string description, string manual, bool discreteBuffer, FlagSchema fSchema) {
		ProgramName = name;
		Description = description;
		Manual = manual;
		WantDiscreteOutputBuffer = discreteBuffer;
		FSchema = fSchema;
	}
}
