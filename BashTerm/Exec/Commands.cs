using BashTerm.Parsers;

namespace BashTerm.Exec;

public abstract record Command;

public record Execve(string name, List<string> args) : Command {
	public override string ToString() {
		return $"Execve[{name}]{{ {string.Join(", ", args)} }}";
	}
};

public record Pipe(Command first, Command post) : Command {
	public override string ToString() {
		return $"PIPE[ {first} | {post} ]";
	}
};

public record Sequence(Command first, Command second) : Command {
	public override string ToString() {
		return $"SEQUENCE[ {first} -> {second} ]";
	}
};

public abstract record VarCommand;

public record VarExecve(string name, List<TokenWord> args) : VarCommand {
	public override string ToString() {
		return $"VarExecve[{name}]{{ {string.Join(", ", args)} }}";
	}
};

public record VarPipe(VarCommand first, VarCommand post) : VarCommand {
	public override string ToString() {
		return $"VarPIPE[ {first} | {post} ]";
	}
};

public record VarSequence(VarCommand first, VarCommand second) : VarCommand {
	public override string ToString() {
		return $"VarSEQUENCE[ {first} -> {second} ]";
	}
};
