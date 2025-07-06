using System;
using System.Collections.Generic;
using RootMotion.FinalIK;

namespace BashTerm.Parsers;

public class FlagParser
{
	public Dictionary<FlagSpec, string?> Flags { get; } = new();
	public List<string> Positionals { get; } = new();

	public FlagParser(List<string> args, FlagSchema schema)
	{
		for (int i = 0; i < args.Count; i++)
		{
			string arg = args[i];

			if (arg == "--") { // Treat all following args as positionals
				for (int j = i + 1; j < args.Count; j++)
				{
					Positionals.Add(args[j]);
				}
				break;
			}

			if (!arg.StartsWith("-"))
			{
				Positionals.Add(arg);
				continue;
			}

			// Normalize
			bool gnuStyle = arg[1] == '-';
			string name = arg.TrimStart('-');
			var spec = gnuStyle ? schema.GetGNU(name) : schema.GetPOSIX(name); // TODO: we want to support posix -abc style, so this needs to be flexible

			if (spec == null)
			{
				throw new FlagException("unknown flag: " + arg);
			}

			if (spec.Type == FlagType.Boolean)
			{
				Flags[spec] = "true";
			}
			else if (spec.Type == FlagType.Value)
			{
				if (i + 1 < args.Count && !args[i + 1].StartsWith("-"))
				{
					Flags[spec] = args[i + 1];
					i++; // Skip value
				}
				else
				{
					throw new FlagException("missing value for flag: " + arg);
				}
			}
		}
	}

	// Example: check if a boolean flag is present, TODO: Change this to be more extensible with Run
	public bool HasFlag(string name, FlagSchema schema) =>
		schema.Get(name) is { } spec && Flags.ContainsKey(spec);

	// Example: get the value for a flag (or null)
	public string? GetFlagValue(string name, FlagSchema schema) =>
		schema.Get(name) is { } spec && Flags.TryGetValue(spec, out var val) ? val : null;
}

public class FlagException : BSHException
{
	public FlagException(string message) : base($"[FlagError] >> {message}") { }
}


