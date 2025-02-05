using Il2CppSystem.Xml.Schema;

namespace BashTerm;

internal static class Util {
	public static string Concat(params string[] args) {
		return Concat(' ', args);
	}

	public static string Concat(char delimiter, params string[] args) {
		return Concat(delimiter, args, 0, args.Length);
	}

	public static string Concat(string[] args, int start, int end) {
		return Concat(' ', args, start, end);
	}

	public static string Concat(char delimiter, string[] args, int start, int end) {
		if (start < 0) start = 0;
		if (end > args.Length) end = args.Length;

		if (args.Length == 0 || start >= end)
			return "";

		string cmd = args[start];
		for (int i = start + 1; i < end; i++)
			cmd += delimiter + args[i];
		return cmd;
	}

	public static string InterpretObjectType(string input) {
		if (string.IsNullOrEmpty(input))
			return input;

		input = input.ToLower();

		// Resource 
		if (input.StartsWith("med"))
			return "medipack";
		if (input.StartsWith("to") || input.StartsWith("tool"))
			return "tool_refill";
		if (input.StartsWith("am") || input.StartsWith("ammo"))
			return "ammopack";
		if (input.StartsWith("dis"))
			return "disinfection_pack";

		// Objective Item 
		if (input.StartsWith("turb") || input == "")
			return "fog_turbine";
		if (input == "nhsu")
			return "neonate_hsu";

		if (input.StartsWith("bk") || input.StartsWith("bulk"))
			return "bulkhead_key";
		if (input.StartsWith("bd"))
			return "bulkhead_dc";

		// Storage
		if (input.StartsWith("lock"))
			return "locker";

        // Doors 
        if (input.StartsWith("sec") || input.StartsWith("sd"))
            return "sec_door";

		// Misc.
		if (input.StartsWith("gen"))
			return "generator";
		if (input == "diss")
			return "disinfection_station";

		// Default: We return what we got
		return input;
	}

	public static bool InterpretResourceType(string input, out string res) {
		res = InterpretObjectType(input);
		return !(res == input);
	}

	public static bool ContainsFlag(params string[] args) {
		foreach (string arg in args)
			if (arg.StartsWith("-"))
				return true;
		return false;
	}

	public static bool IsInt(string input) { 
		return int.TryParse(input, out int result);
	}
}
