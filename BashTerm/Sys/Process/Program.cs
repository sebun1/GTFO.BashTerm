using System.Collections;
using Bsh.Exec;
using Bsh.Parsers;
using Bsh.Sys.Stream;
using Bsh.Types;
using UnityEngine;

namespace Bsh.Sys.Process;

public enum EProgramState {
	Active,
	Inactive,
	Exited
}

/// <summary>
/// <c>Program</c> - Defines a base program structure for all Bsh processes.
/// <para>
/// All Program should implement a
///	<code>public static ProgramManifest GetManifest()</code>
/// function to be registered with BshSystem at startup. If
/// unspecified, the process will not be registered and thus not callable
/// </para>
/// </summary>
public abstract class Program : IProcess {
	public int Pid => Ctx?.Pid ?? -1;

	/// <summary>
	/// Whether the program has been initialized.
	/// </summary>
	private bool _initialized;

	/// <summary>
	/// Program context of the running instance.
	/// Guaranteed to be non-null during Run/Start/Update and signal calls.
	/// </summary>
	protected ProgramContext? Ctx;

	/// <summary>
	/// Reference to Terminal in program context.
	/// Guaranteed to be non-null during Run/Start/Update and signal calls.
	/// </summary>
	protected Terminal Term => Ctx?.Terminal!;

	/// <summary>
	/// Reference to list of arguments in program context.
	/// Guaranteed to be non-null during Run/Start/Update and signal calls.
	/// </summary>
	protected List<string> Args => Ctx?.Args!;

	/// <summary>
	/// Reference to commandline options in program context.
	/// Guaranteed to be non-null during Run/Start/Update and signal calls.
	/// </summary>
	protected CmdOpts Opts => Ctx?.Opts!;

	/// <summary>
	/// Reference to standard input stream in program context.
	/// Guaranteed to be non-null during Run/Start/Update and signal calls.
	/// </summary>
	protected PipeStreamReader<byte> StdIn => Ctx?.StdIn!;

	/// <summary>
	/// Reference to standard output stream in program context.
	/// Guaranteed to be non-null during Run/Start/Update and signal calls.
	/// </summary>
	protected PipeStreamWriter<byte> StdOut => Ctx?.StdOut!;

	/// <summary>
	/// Reference to object input stream in program context.
	/// Guaranteed to be non-null during Run/Start/Update and signal calls.
	/// </summary>
	protected PipeStreamReader<PipeObject> ObjIn => Ctx?.ObjIn!;

	/// <summary>
	/// Reference to object output stream in program context.
	/// Guaranteed to be non-null during Run/Start/Update and signal calls.
	/// </summary>
	protected PipeStreamWriter<PipeObject> ObjOut => Ctx?.ObjOut!;

	/// <summary>
	/// For program exit behaviors.
	/// </summary>
	public event EventHandler? OnExit;

	/// <summary>
	/// State of the program.
	/// </summary>
	public EProgramState State { get; private set; } = EProgramState.Inactive;

	/// <summary>
	/// Sets the program state to Active.
	/// </summary>
	internal void SetActive() {
		State = EProgramState.Active;
	}

	/// <summary>
	/// Sets the program state to Inactive.
	/// </summary>
	internal void SetInactive() {
		State = EProgramState.Inactive;
	}

	/// <summary>
	/// Used to initialize the program with its context.
	/// </summary>
	/// <param name="ctx">context of the running instance</param>
	/// <exception cref="BshSystemException">if program is already initialized</exception>
	internal void Init(ProgramContext ctx) {
		if (_initialized)
			throw new BshSystemException("Program already initialized");
		Ctx = ctx;
		OnExit += (_, _) => { State = EProgramState.Exited; };
		_initialized = true;
	}

	/// <summary>
	/// Method to signal program exit. Should be called when the program is ready to terminate.
	/// </summary>
	protected void Exit() {
		OnExit?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// For coroutine-based programs, the main execution loop.
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerator Run() {
		yield return null;
	}

	/// <summary>
	/// For programs that use Start/Update model, called once at the start.
	/// </summary>
	public virtual void Start() {
	}

	/// <summary>
	/// For programs that use Start/Update model, called once per frame.
	/// </summary>
	public virtual void Update() {
	}

	/// <summary>
	/// Invoked when the program receives a SIGINT (Ctrl+C).
	/// </summary>
	public void SigInt() {
		if (OnSigInt()) Exit();
	}

	/// <summary>
	/// Invoked when the program receives a SIGINT (Ctrl+C).
	/// Can be overridden to customize behavior. Return false to ignore termination.
	/// <returns>whether the program should terminate</returns>
	/// </summary>
	public virtual bool OnSigInt() {
		return true;
	}

	/// <summary>
	/// Invoked when the program receives a SIGTSTP (Ctrl+Z).
	/// </summary>
	public void SigStp() {
		OnSigStp();
		// TODO: Implement suspension logic
	}

	/// <summary>
	/// Invoked when the program receives a SIGTSTP (Ctrl+Z).
	/// Can be overridden to customize behavior.
	/// </summary>
	public virtual void OnSigStp() {
	}

	/// <summary>
	/// Invoked when the program receives a SIGQUIT (Ctrl+\).
	/// </summary>
	public void SigQuit() {
		Exit();
	}
}

public class ProgramManifest {
	/// <summary>
	/// A brief description of the program.
	/// </summary>
	public string Description;

	/// <summary>
	/// The manual for the program.
	/// </summary>
	public string Manual;

	/// <summary>
	/// Whether the program uses an alternate pane for rendering.
	/// </summary>
	public bool AlternatePane;

	/// <summary>
	/// Whether the program would like to process raw input (keystrokes), rather
	/// than using the canonical line buffer.
	/// </summary>
	public bool RawInput;

	/// <summary>
	/// Whether the program uses coroutine-based execution.
	/// If false, Start/Update is used instead.
	/// </summary>
	public bool UseCoroutine;

	/// <summary>
	/// Flag schema for the program's command-line arguments.
	/// </summary>
	public FlagSchema FSchema;

	public ProgramManifest(string description, string manual, FlagSchema fSchema,
		bool alternatePane = false, bool rawInput = false, bool useCoroutine = true) {
		Description = description;
		Manual = manual;
		FSchema = fSchema;
		AlternatePane = alternatePane;
		RawInput = rawInput;
		UseCoroutine = useCoroutine;
	}
}
