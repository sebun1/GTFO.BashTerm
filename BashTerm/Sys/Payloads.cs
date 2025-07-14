using BashTerm.Exec;
using LevelGeneration;

namespace BashTerm.Sys;

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
	public int Code { get; }
	public string Message { get; }
	public PipedPayload Payload { get; }

	public ExitPayload(int code, string message, PipedPayload? payload) {
		Code = code;
		Message = message;
		Payload = payload ?? new EmptyPayload();
	}
}
