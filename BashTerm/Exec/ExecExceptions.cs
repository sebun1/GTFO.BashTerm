namespace BashTerm.Exec;

public class ExecException : Exception {

	public ExecException(string cause) : base(cause) {}

	public override string ToString() => $"[ExecError] >> {Message}";
}

public class CommandException : ExecException {
	public string CommandName { get; }

	public CommandException(string command, string cause) : base(cause) {
		CommandName = command;
	}

	public override string ToString() => $"[CommandError] ({CommandName}) >> {Message}";
}

internal class UnknownParserCommandTypeException : ExecException {
	public UnknownParserCommandTypeException(Type t)
		: base($"type {t.FullName} is not a valid parser command type") {}
}

internal class TooManyArgumentsException : CommandException {
	public TooManyArgumentsException(string commandName, int got, int expected)
		: base(commandName, $"command received too many arguments, want {expected}, got {got}") {
	}
}

internal class MissingArgumentException : CommandException {
	public MissingArgumentException(string commandName, int got, int expected)
		: base(commandName, $"command '{commandName}' is missing arguments, want {expected}, got {got}") {
	}
}

internal class NullTerminalInstanceException : CommandException {
	public NullTerminalInstanceException(string commandName)
		: base(commandName, $"command was called, but no valid LG_ComputerTerminal instance could be found") {
	}
}
