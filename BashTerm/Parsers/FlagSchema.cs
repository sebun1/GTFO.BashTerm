namespace BashTerm.Parsers;

public enum FlagType {
	Boolean,
	Value
}

public record FlagSpec(string POSIXName, string? GNUName, FlagType Type);

public class FlagSchema {
	private readonly Dictionary<string, FlagSpec> _gnu = new();
	private readonly Dictionary<string, FlagSpec> _posix = new();

	public void Add(string posixName, string? gnuName, FlagType type) {
		var spec = new FlagSpec(posixName, gnuName, type);
		_posix[posixName] = spec;
		if (gnuName != null)
			_gnu[gnuName] = spec;
	}

	public FlagSpec? GetGnu(string name) => _gnu.GetValueOrDefault(name);

	public FlagSpec? GetPosix(string name) => _posix.GetValueOrDefault(name);
}
