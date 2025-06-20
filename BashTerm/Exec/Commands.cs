namespace BashTerm.Exec;

public abstract record Command;

public record Execve(string name, List<string> args) : Command;

public record RawCommand() : Command;

public record EmptyCommand() : Command;

public record Pipe(Command first, Command post) : Command;

public record Sequence(Command first, Command second) : Command;

/*
public record ListCommand(List<string> args) : Command;

public record QueryCommand(List<string> items) : Command;

public record PingCommand(List<string> items) : Command;

public record StartCommand(string protocol) : Command;

public record ReactorStartCommand() : Command;

public record ReactorStopCommand() : Command;

public record ReactorVerifyCommand(string code) : Command;

public record UplinkStartCommand(string address) : Command;

public record UplinkVerifyCommand(string code) : Command;

public record UplinkConfirmCommand() : Command;

public record LogsCommand() : Command;

public record ReadCommand(string file) : Command;

public record InfoCommand() : Command;

public record HelpCommand() : Command;

public record ExitCommand() : Command;

public record ClearCommand() : Command;

public record SpecialCommand(string raw) : Command;
*/
