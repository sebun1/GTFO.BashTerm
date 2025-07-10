using BashTerm.Parsers;
using BashTerm.Utils;

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

public abstract record VarCommand : IFmtToStringable {
	public abstract string FmtToString();
};

public record VarExecve(TokenWord name, List<TokenWord> args) : VarCommand {
	public override string ToString() {
		return $"[execve {name}]\n  {Fmt.Indent(string.Join("\n", args), 2)}";
	}

	public override string FmtToString() {
		return $"<#FD971F>[EXEC]<<#F92672>{name.FmtToString()}</color>></color> <#A6E22E>{{\n  <#E6DB74>{Fmt.Indent(string.Join("\n", args.Select(p => p.FmtToString())), 2)}</color>\n}}</color>";
	}
};

public record VarPipe(VarCommand first, VarCommand post) : VarCommand {
	public override string ToString() {
		return $"{first}\n|\n{post}";
	}

	public override string FmtToString() {
		return $"<#AE81FF>[PIPE]</color>\n  {Fmt.Indent(first)}\n  <#AE81FF>|</color>\n  {Fmt.Indent(post)}";
	}
};

public record VarSequence(VarCommand first, VarCommand second) : VarCommand {
	public override string ToString() {
		return $"{first}\n;\n{second}";
	}

	public override string FmtToString() {
		return $"<#AE81FF>[SEQUENCE]</color>\n  {Fmt.Indent(first)}\n  <#AE81FF>;</color>\n  {Fmt.Indent(second)}";
	}
};
