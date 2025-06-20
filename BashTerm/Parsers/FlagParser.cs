using System;
using System.Collections.Generic;

namespace BashTerm.Parsers;

internal class FlagParser {
	private readonly Dictionary<string, string> _flags = new(StringComparer.OrdinalIgnoreCase);

	public FlagParser(List<string> args) {
		for (int i = 0; i < args.Count - 1; i++) {
			string key = args[i];
			string value = args[i + 1];

			if (key.StartsWith("-") && !value.StartsWith("-")) {
				_flags[key] = value;
				i++; // Skip value
			}
		}
	}

	public string? GetVal(string primary, string? fallback = null) {
		if (_flags.TryGetValue(primary, out var val)) return val;
		if (fallback != null && _flags.TryGetValue(fallback, out val)) return val;
		return null;
	}
}
