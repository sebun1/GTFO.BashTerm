using BashTerm.Exec;
using BashTerm.Parsers;
using LevelGeneration;

namespace BashTerm.Sys;

public class StartPayload {
	public readonly List<string> Args;
	public readonly CmdOpts Opts;
	public readonly PipeObject Payload;

	public StartPayload(List<string> args, CmdOpts opts, PipeObject payload) {
		Args = args;
		Opts = opts;
		Payload = payload;
	}
}

public class UpdatePayload {
	public bool HasPayload => Payload is not EmptyObject;
	public PipeObject Payload { get; }

	public bool HasLine { get; }
	public string Line { get; }

	public bool HasChar { get; }
	public char Char { get; }

	public UpdatePayload(PipeObject? payload, string? line, char? c) {
		Payload = payload ?? new EmptyObject();
		HasLine = line != null;
		Line = line ?? "";
		HasChar = c != null;
		Char = c ?? '\0';
	}
}

public class ExitPayload {
	public readonly int Code;
	public readonly string Message;
	public readonly PipeObject Payload;

	public ExitPayload() {
		Code = 0;
		Message = "";
		Payload = new EmptyObject();
	}

	public ExitPayload(PipeObject? payload) {
		Code = 0;
		Message = "";
		Payload = payload ?? new EmptyObject();
	}

	public ExitPayload(int code, string message, PipeObject? payload) {
		Code = code;
		Message = message;
		Payload = payload ?? new EmptyObject();
	}

}
