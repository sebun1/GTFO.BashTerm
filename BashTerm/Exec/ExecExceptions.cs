using BashTerm.Runtime;

namespace BashTerm.Exec;

public class ExecException : BSHException {

	public ExecException(string cause) : base(cause) {}

	public override string ToString() => $"[ExecError] >> {Message}";
}

public class CmdRunException : ExecException {
	public CmdRunException(string cause) : base($"[CmdRunException] >> {cause}") {}
}

public class CommandException : ExecException {
	public string CommandName { get; }

	public CommandException(string command, string cause) : base($"[CommandError] ({command}) >> {cause}") {
		CommandName = command;
	}
}

public class UnknownParserCommandTypeException : ExecException {
	public UnknownParserCommandTypeException(Type t)
		: base($"type {t.FullName} is not a valid parser command type") {}
}

public class TooManyArgumentsException : CommandException {
	public TooManyArgumentsException(string commandName, int got, int expected)
		: base(commandName, $"command received too many arguments, want {expected}, got {got}") {
	}
}

public class MissingArgumentException : CommandException {
	public MissingArgumentException(string commandName, int got, int expected)
		: base(commandName, $"command is missing arguments, want {expected}, got {got}") {
	}
}

public class NullTerminalInstanceException : CommandException {
	public NullTerminalInstanceException(string commandName)
		: base(commandName, $"command was called, but no valid LG_ComputerTerminal instance was provided") {
	}
}
