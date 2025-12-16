using Bsh.Parsers;
using Bsh.Sys;
using Bsh.Sys.Process;
using Bsh.Sys.Stream;

namespace Bsh.Exec.Builtins;

[BshProgram("hello")]
public class Hello : Program {
	private const string Name = "hello";
	private const string Desc = "say hello to someone";

	private const string Manual = "" +
	                              "\x1B[34m<b>NAME</b>\n" +
	                              "		hello - say hello to someone\n" +
	                              "\n" +
	                              "<b>USAGE</b>\n" +
	                              "		hello <u>NAME</u> [-p/--prefix <u>name prefix</u>]\n" +
	                              "\n" +
	                              "<b>OPTIONS</b>\n" +
	                              "		-p, --prefix\n" +
	                              "\n";

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
		return new(Desc, Manual, FSchema, RequestAlternateBuffer, false, false);
	}

	public override void Start() {
		_reader = new TextStreamReader(Ctx!.StdIn);
		_writer = new TextStreamWriter(Ctx!.StdOut);
		if (Ctx.Args.Count == 0)
			_writer.TryWriteLine("Hello... but you didn't give me a name!");
		else
			_writer.TryWriteLine($"Hello, {string.Join(" ", Args)}!");
		_writer.TryWriteLine("Ctrl-C twice to exit.");
	}

	public override bool OnSigInt() {
		if (_firstSigInt) {
			_firstSigInt = false;
			_writer.TryWriteLine("Hello program received SIGINT, press again to exit...");
			return false;
		}

		_writer.TryWriteLine("Hello program received SIGINT, exiting...");
		return true;
	}

	public override void Update() {
	}
}
