using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using Dissonance;
using Il2CppSystem.Linq;

namespace BashTerm;

internal static class ConfigMaster {
	public static ConfigFile conf;

	//public static ConfigEntry<int> CONFIG_NOTICE;

	public static ConfigEntry<bool> _EnableRVAlias;
	public static ConfigEntry<bool> _EnableUVAlias;
	public static ConfigEntry<bool> _LsObjExpansions;
	public static ConfigEntry<bool> _LsConvertsNum2ZoneId;

	public static ConfigEntry<string> CustomCmdAliases;
	public static ConfigEntry<string> CustomObjExpansions;

	public static ConfigEntry<bool> _DEBUG;

	public static Dictionary<string, string> CmdExpExact = new Dictionary<string, string> {
		{ "ls", "list" },
		{ "l", "list" },
		{ "lsu", "list u"},
		{ "lu", "list u" },
		{ "uc", "uplink_connect" },
		{ "uv", "uplink_verify" },
		{ "rs", "reactor_startup" },
		{ "start", "reactor_startup" },
		{ "rsd", "reactor_shutdown" },
		{ "shut", "reactor_shutdown" },
		{ "shutdown", "reactor_shutdown" },
		{ "rv", "reactor_verify" },
		{ "cat", "read"},
		{ "p", "ping" },
		{ "q", "query" },
		{ "clear", "cls" },
		{ "r", "raw" },
	};

	public static List<(string Prefix, string Expansion)> CmdExpPrefix = new List<(string Prefix, string Expansion)> { };

	public static Dictionary<string, string> ObjExpExact = new Dictionary<string, string> {
		{ "nhsu", "neonate_hsu" },
		{ "diss", "disinfection_station" },
	};

	public static List<(string Prefix, string Expansion)> ObjExpPrefix = new List<(string Prefix, string Expansion)> {
		( "med", "medipack" ),
		( "to", "tool_refill" ),
		( "am", "ammopack" ),
		( "dis", "disinfect_pack" ),
		( "turb", "fog_turbine" ),
		( "bk", "bulkhead_key" ),
		( "bulk", "bulkhead_key" ),
		( "bd", "bulkhead_dc" ),
		( "his", "hisec_cargo" ),
		( "dc", "data_cube" ),
		( "data", "data_cube" ),
		( "lock", "locker" ),
		( "sec", "sec_door" ),
		( "sd", "sec_door" ),
		( "nfr", "nframe" ),
		( "gen", "generator" ),
	};

	static ConfigMaster() {
		string cfgPath = Path.Combine(Paths.ConfigPath, $"{Plugin.NAME}.cfg");
		conf = new ConfigFile(cfgPath, true);

		string sect;
		int sect_num = 1;

		sect = $"({sect_num++}) Aliases";

		_EnableRVAlias = conf.Bind(
			sect,
			"Enable REACTOR_VERIFY Alias",
			true,
			"By Default Maps \"RV\" to \"REACTOR_VERIFY\""
		);

		_EnableUVAlias = conf.Bind(
			sect,
			"Enable UPLINK_VERIFY Alias",
			true,
			"By Default Maps \"UV\" to \"UPLINK_VERIFY\""
		);

		sect = $"({sect_num++}) Advanced Aliases / Expansions";

		_LsObjExpansions = conf.Bind(
			sect,
			"LS Performs Object Name Expansion",
			false,
			"Attempts to expand first non-number argument for LS (e.g. LIST TOOL TOOL to LIST TOOL_REFILL TOOL)"
		);

		_LsConvertsNum2ZoneId = conf.Bind(
			sect,
			"LS Converts Number to Zone ID",
			false,
			"Converts first integer argument for LS to zone identifier (e.g. LS 49 50 to LS E_49 50)"
		);

		CustomCmdAliases = conf.Bind(
			sect,
			"Custom Command Aliases",
			"",
			"Add custom command aliases here.\n" +
				"Must match Format: \"<command expression>,<alias1>,<alias2>:<command>,<alias1>\"\n" +
				"Prefix Match: \"Pre+\", Exact Match: \"Exact\"" +
				"e.g. \"list u, lsu, lu: uplink_verify, uv+\"\n" +
				"Note: Terms are case-insensitive. Newer definitions override older ones, including defaults. Check README on GitHub/GitLab for details."
		);

		CustomObjExpansions = conf.Bind(
			sect,
			"Custom Object Name Expansions",
			"",
			"Add custom object name expansions here.\n" +
				"Must match Format: \"<expansion1>,<identifier1>,<identifier2>:<expansion2>,<identifier1>\"\n" +
				"Prefix Match: \"Pre+\", Exact Match: \"Exact\"" +
				"e.g. \"get the fk out, gtfo, gtfi: my_custom_object, mco+\"\n" +
				"Note: Terms are case-insensitive. Newer definitions override older ones, including defaults. Check README on GitHub/GitLab for details."
		);

		sect = $"(Z) Dev";

		_DEBUG = conf.Bind(sect, "Enable Debugging", false, "Enables Debug Logging");
	}

	public static void Init() {
		int default_count = GetExpansionCount();
		int override_count = 0;
		override_count += LoadExpansionPairs(CustomCmdAliases.Value, ref CmdExpExact, ref CmdExpPrefix);
		override_count += LoadExpansionPairs(CustomObjExpansions.Value, ref ObjExpExact, ref ObjExpPrefix);
		int total_count = GetExpansionCount();
		Logger.Info($"Aliases/Expansions Loaded: {default_count} Default ({override_count} Prefix Overrides) + {total_count - default_count} User = {total_count} Total");
		Logger.Info("Config is Loaded");
	}

	public static int GetExpansionCount() {
		return CmdExpExact.Count
			+ CmdExpPrefix.Count
			+ ObjExpExact.Count
			+ ObjExpPrefix.Count;
	}

	public static int LoadExpansionPairs(string source, ref Dictionary<string, string> targetExact, ref List<(string Prefix, string Expansion)> targetPrefix) {
		var groups = ParseUtil.FromUserDefGroups(source);
		int replaced = 0;

		foreach (List<string> group in groups) {
			string expansion = group[0];
			if (group.Count < 2 || string.IsNullOrWhiteSpace(expansion)) {
				continue;
			}

			for (int i = 1; i < group.Count; i++) {
				string term = group[i];
				if (string.IsNullOrWhiteSpace(term)) {
					continue;
				}


				if (term.EndsWith("+")) {
					term = term.Substring(0, term.Length - 1);
					int existentIndex = targetPrefix.FindIndex(x => x.Prefix == term);
					if (existentIndex != -1) {
						targetPrefix[existentIndex] = (term, expansion);
						replaced++;
						Logger.Debug("Overwriting Prefix Expansion: (" + term + " -> " + expansion + ")");
					} else {
						targetPrefix.Add((term, expansion));
					}
				} else {
					targetExact[term] = expansion;
				}
			}
		}

		targetPrefix.Sort((a, b) => b.Prefix.Length.CompareTo(a.Prefix.Length));

		return replaced;
	}

	public static bool EnableRVAlias {
		get { return _EnableRVAlias.Value; }
	}

	public static bool EnableUVAlias {
		get { return _EnableUVAlias.Value; }
	}

	public static bool DEBUG {
		get { return _DEBUG.Value; }
	}

	public static bool LsObjExpansions {
		get { return _LsObjExpansions.Value; }
	}

	public static bool LsConvertsNum2ZoneId {
		get { return _LsConvertsNum2ZoneId.Value; }
	}
}
