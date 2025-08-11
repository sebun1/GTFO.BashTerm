using BashTerm.Parsers;
using BashTerm.Sys;
using LevelGeneration;

namespace BashTerm.Exec.Procs;

public class Hello : Proc {
	private const string Name = "query";
	private const string Desc = "Queries the location of a items";
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

	private static FlagSchema CreateFlagSchema() {
		FlagSchema fs = new FlagSchema();
		fs.Add("p", "prefix", FlagType.Value);
		return fs;
	}

	public static ProcManifest GetManifest() {
		return new ProcManifest(Name, Desc, Manual, RequestAlternateBuffer, FSchema);
	}

	public override void Start(StartPayload payload, LG_ComputerTerminal term) {

	}

	public override void Update(UpdatePayload payload) {
		payload.
	}
}
