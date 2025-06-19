namespace BashTerm.Parsers;

abstract record Command;

record Execve(string name, List<string> args) : Command;

record ListCommand(List<string> args) : Command;

record QueryCommand(List<string> items) : Command;

record PingCommand(List<string> items) : Command;

record StartCommand(string protocol) : Command;

record ReactorStartCommand() : Command;

record ReactorStopCommand() : Command;

record ReactorVerifyCommand(string code) : Command;

record UplinkStartCommand(string address) : Command;

record UplinkVerifyCommand(string code) : Command;

record UplinkConfirmCommand() : Command;

record LogsCommand() : Command;

record ReadCommand(string file) : Command;

record InfoCommand() : Command;

record HelpCommand() : Command;

record ExitCommand() : Command;

record ClearCommand() : Command;

record RawCommand() : Command;

record EmptyCommand() : Command;

record SpecialCommand(string raw) : Command;

record Pipe(Command first, Command post) : Command;

record Sequence(Command first, Command second) : Command;
