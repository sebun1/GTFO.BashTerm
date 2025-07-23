using BashTerm.Exec;
using BashTerm.Parsers;
using LevelGeneration;

namespace BashTerm.Sys;

public class StartPayload {
	public readonly List<string> Args;
	public readonly CmdOpts Opts;
	public readonly PipedPayload Payload;

	public StartPayload(List<string> args, CmdOpts opts, PipedPayload payload) {
		Args = args;
		Opts = opts;
		Payload = payload;
	}
}

public class UpdatePayload {
	public bool HasPayload => Payload is not EmptyPayload;
	public PipedPayload Payload { get; }

	public bool HasLine { get; }
	public string Line { get; }

	public bool HasChar { get; }
	public char Char { get; }

	public UpdatePayload(PipedPayload? payload, string? line, char? c) {
		Payload = payload ?? new EmptyPayload();
		HasLine = line != null;
		Line = line ?? "";
		HasChar = c != null;
		Char = c ?? '\0';
	}
}

public class ExitPayload {
	public readonly int Code;
	public readonly string Message;
	public readonly PipedPayload Payload;

	public ExitPayload() {
		Code = 0;
		Message = "";
		Payload = new EmptyPayload();
	}

	public ExitPayload(PipedPayload? payload) {
		Code = 0;
		Message = "";
		Payload = payload ?? new EmptyPayload();
	}

	public ExitPayload(int code, string message, PipedPayload? payload) {
		Code = code;
		Message = message;
		Payload = payload ?? new EmptyPayload();
	}

}
