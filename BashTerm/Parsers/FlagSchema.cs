namespace BashTerm.Parsers;

public enum FlagType { Boolean, Value }

public record FlagSpec(string POSIXName, string? GNUName, FlagType Type);

public class FlagSchema
{
	private readonly Dictionary<string, FlagSpec> _gnu = new();
	private readonly Dictionary<string, FlagSpec> _posix = new();

	public void Add(string posixName, string? gnuName, FlagType type)
	{
		var spec = new FlagSpec(posixName, gnuName, type);
		_posix[posixName] = spec;
		if (gnuName != null)
			_gnu[gnuName] = spec;
	}

	public FlagSpec? Get(string name) =>
		_gnu.TryGetValue(name, out var spec) ? spec :
		_posix.TryGetValue(name, out spec) ? spec :
		null;

	public FlagSpec? GetGNU(string name) =>
		_gnu.TryGetValue(name, out var spec) ? spec : null;

	public FlagSpec? GetPOSIX(string name) =>
		_posix.TryGetValue(name, out var spec) ? spec : null;
}

