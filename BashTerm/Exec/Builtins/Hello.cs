using BashTerm.Parsers;
using BashTerm.Sys;
using BashTerm.Sys.Stream;

namespace BashTerm.Exec.Builtins;

[BshProgram("hello")]
public class Hello : Program {
	private const string Name = "hello";
	private const string Desc = "say hello to someone";

	private const string Manual = @"
<b>NAME</b>
		hello - say hello to someone

<b>USAGE</b>
		hello <u>NAME</u> [-p/--prefix <u>name prefix</u>]

<b>OPTIONS</b>
		-p, --prefix

";

	private const bool RequestAlternateBuffer = false;

	private static readonly FlagSchema FSchema = CreateFlagSchema();

	private bool _firstSigInt = true;

	private TextStreamReader _reader;
	private TextStreamWriter _writer;

	private static FlagSchema CreateFlagSchema() {
		FlagSchema fs = new FlagSchema();
		fs.Add("p", "prefix", FlagType.Value);
		return fs;
	}

	public static ProgramManifest GetManifest() {
		return new(Name, Desc, Manual, RequestAlternateBuffer, FSchema);
	}

	public override void Start() {
		_reader = new TextStreamReader(Ctx!.StdIn);
		_writer = new TextStreamWriter(Ctx!.StdOut);
		if (Ctx.Args.Count == 0)
			_writer.TryWriteLine("Hello... but you didn't give me a name!");
		else
			_writer.TryWriteLine($"Hello, {string.Join(" ", Ctx.Args)}!");
		_writer.TryWriteLine("Ctrl-C twice to exit.");
	}

	public override void OnSigInt() {
		if (_firstSigInt) {
			_firstSigInt = false;
			_writer.TryWriteLine("Hello program received SIGINT, press again to exit...");
		}

		_writer.TryWriteLine("Hello program received SIGINT, exiting...");
		Exit();
	}

	public override void Update() {
	}
}
