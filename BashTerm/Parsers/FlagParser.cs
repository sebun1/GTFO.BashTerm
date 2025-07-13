using BashTerm.Sys;

namespace BashTerm.Parsers;

public class FlagParser {
	public Dictionary<FlagSpec, string> Flags { get; } = new();
	public List<string> Positionals { get; } = new();

	public FlagParser(List<string> args, FlagSchema schema) {
		for (int i = 0; i < args.Count; i++) {
			string arg = args[i];

			if (arg == "--") {
				for (int j = i + 1; j < args.Count; j++) {
					Positionals.Add(args[j]);
				}
				break;
			}

			if (!arg.StartsWith("-")) {
				Positionals.Add(arg);
				continue;
			}

			bool gnuStyle = arg[1] == '-';
			string name = arg.TrimStart('-');
			FlagSpec? spec;
			if (gnuStyle) {
				spec = schema.GetGnu(name);
				if (spec == null)
					throw new FlagException($"unknown flag: {arg}");
				if (spec.Type == FlagType.Boolean) {
					Flags[spec] = "true";
				} else if (spec.Type == FlagType.Value) {
					if (i + 1 < args.Count && !args[i + 1].StartsWith("-")) {
						Flags[spec] = args[i + 1];
						i++;
					} else {
						throw new FlagException($"missing value for flag: {arg}");
					}
				}
			} else {
				foreach (char c in name) {
					spec = schema.GetPosix($"{c}");
					if (spec == null)
						throw new FlagException($"unknown flag: {c} in {arg}");
					if (spec.Type == FlagType.Boolean) {
						Flags[spec] = "true";
					} else if (spec.Type == FlagType.Value) {
						if (i + 1 < args.Count && !args[i + 1].StartsWith("-")) {
							Flags[spec] = args[i + 1];
							i++;
						} else {
							throw new FlagException($"missing value for flag: {c} in {arg}");
						}
					}
				}
			}
		}
	}
}

public class CmdOpts {
	private Dictionary<string, string> _posix = new();
	private Dictionary<string, string> _gnu = new();

	public CmdOpts(Dictionary<FlagSpec, string> flags) {
		foreach (var (spec, val) in flags) {
			_posix[spec.POSIXName] = val;
			if (spec.GNUName != null) _gnu[spec.GNUName] = val;
		}
	}

	public bool TryGetPosix(string flag, out string? val) {
		return _posix.TryGetValue(flag, out val);
	}

	public bool TryGetGnu(string flag, out string? val) {
		return _gnu.TryGetValue(flag, out val);
	}

	/// <summary>
	/// Given argument of flag form (gnu style <c>--flag</c>, posix style <c>-f</c>)
	/// returns value (or "true" for boolean, from parser) if flag exists, otherwise null.
	/// Throws if non-flag type argument provided.
	/// </summary>
	public string? this[string flagArg] {
		get {
			if (flagArg[0] != '-') throw new CmdOptException("indexing non-flag value (does not start with '-')");
			bool gnuStyle = flagArg[1] == '-';
			string name = flagArg.TrimStart('-');
			return (gnuStyle ? _gnu : _posix).GetValueOrDefault(name);
		}
	}

	public static CmdOpts EmptyOpts() {
		return new CmdOpts(new  Dictionary<FlagSpec, string>());
	}
}

public class FlagException : BSHException {
	public FlagException(string message) : base($"[FlagError] >> {message}") {
	}
}

public class CmdOptException : BSHException {
	public CmdOptException(string message) : base($"[CmdOptError] >> {message}") {
	}
}
