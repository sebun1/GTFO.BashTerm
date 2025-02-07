using System;
using System.IO;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using Il2CppSystem.Linq;

namespace BashTerm;

internal static class ConfigMaster {
	public static ConfigFile conf;

	public static ConfigEntry<bool> _DEBUG;

	public static ConfigEntry<bool> _EnableRVAlias;
	public static ConfigEntry<bool> _EnableUVAlias;
	public static ConfigEntry<bool> _LsObjExpansions;
	public static ConfigEntry<bool> _LsConvertsNum2ZoneId;

	public static ConfigEntry<string> CustomCmdAliases;
	public static ConfigEntry<string> CustomObjExpansions;

	public static Dictionary<TermCmd, HashSet<string>> CmdIdentifiers = new Dictionary<TermCmd, HashSet<string>> {
		// NOTE: If we want to add the ability to disable all default aliases, we have to persist detection of
		// commands like PING or QUERY for expansion/concat functionality, perhaps another persistent dictionary?
		{ TermCmd.ShowList, new HashSet<string>{ "l", "ls" } },
		{ TermCmd.ShowListU, new HashSet<string>{ "lsu", "lu" } },
		{ TermCmd.TerminalUplinkConnect, new HashSet<string>{ "uc" } },
		{ TermCmd.TerminalUplinkVerify, new HashSet<string>{ "uv" } },
		{ TermCmd.ReactorStartup, new HashSet<string>{ "rs", "start" } },
		{ TermCmd.ReactorShutdown, new HashSet<string>{ "rsd", "shut", "shutdown" } },
		{ TermCmd.ReactorVerify, new HashSet<string>{ "rv" } },
		{ TermCmd.ReadLog, new HashSet<string>{ "cat" } },
		{ TermCmd.Ping, new HashSet<string>{ "p", "ping" } },
		{ TermCmd.Query, new HashSet<string>{ "q", "query" } },
		{ TermCmd.Cls, new HashSet<string>{ "clear" } },
		{ TermCmd.Raw, new HashSet<string>{ "raw", "r" } },
	};

	public static Dictionary<string, HashSet<string>> ObjExpansions = new Dictionary<string, HashSet<string>> {
		// Resource
		{ "medipack", new HashSet<string> { "med+" } },
		{ "tool_refill", new HashSet<string>{ "to+" } },
		{ "ammopack" , new HashSet<string>{ "am+" } },
		{ "disinfection_pack" , new HashSet<string>{ "dis+" } },

		// Objective Items
		{ "fog_turbine" , new HashSet<string>{ "turb+" } },
		{ "neonate_hsu" , new HashSet<string>{ "nhsu+" } },
		{ "bulkhead_key" , new HashSet<string>{ "bk+", "bulk+" } },
		{ "bulkhead_dc" , new HashSet<string>{ "bd+" } },
		{ "hisec_cargo" , new HashSet<string>{ "his+" } },
		{ "data_cube" , new HashSet<string>{ "dc+", "data+" } },

		// Storage
		{ "locker" , new HashSet<string>{ "lock+" } },

		// Doors
		{ "sec_door" , new HashSet<string>{ "sec+", "sd+" } },

		// Misc.
		{ "nframe" , new HashSet<string>{ "nfr+" } },
		{ "generator" , new HashSet<string>{ "gen+" } },
		{ "disinfection_station" , new HashSet<string>{ "diss" } },
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
			"Attempts to expand first non-number argument for LS (e.g. TOOL to TOOL_REFILL)"
		);

		_LsConvertsNum2ZoneId = conf.Bind(
			sect,
			"LS Converts Number to Zone ID",
			false,
			"Converts first integer argument for LS to zone identifier (e.g. 49 to E_49)"
		);

		CustomCmdAliases = conf.Bind(
			sect,
			"Custom Command Aliases",
			"",
			"Add custom command aliases here. Commands must match Format: <command1>,<alias1>,<alias2>;<command1>,<alias1>,<alias2>\ne.g. \"ls,dir;list\""
		);

		CustomObjExpansions = conf.Bind(
			sect,
			"Custom Command Aliases",
			"",
			"Add custom command aliases here. Commands must match Format: <command1>,<alias1>,<alias2>;<command1>,<alias1>,<alias2>\ne.g. \"ls,dir;list\""
		);

		sect = $"({sect_num++}) Dev";

		_DEBUG = conf.Bind(sect, "Enable Debugging", false, "Enables Debug Logging");
	}

	public static void Init() {
		LoadCustomAlias();
		LoadCustomExpansion();
	}

	public static void LoadCustomAlias() {

	}

	public static void LoadCustomExpansion() {
		string rawCustomObjExp = CustomObjExpansions.Value;
		if (string.IsNullOrEmpty(rawCustomObjExp)) {
			return;
		}

		string[] userObjExpansions = rawCustomObjExp.Split(';');
		foreach (string userObjExp in userObjExpansions) {
			string[] objExpParts = userObjExp.Split(',').Select(p => p.Trim()).ToArray();
			if (ObjExpansions.ContainsKey(objExpParts[0])) {
				var set = ObjExpansions[objExpParts[0]];
				for (int i = 1; i < objExpParts.Length; i++) {
					set.Add(objExpParts[i]);
				}
			} else {
				HashSet<string> newExpansionList = new HashSet<string>();
				for (int i = 1; i < objExpParts.Length; i++) {
					newExpansionList.Add(objExpParts[i]);
				}
				ObjExpansions.Add(objExpParts[0], newExpansionList);
			}
		}
	}

	public static void ConfigInfo() {
		Logger.Debug("ObjExpansion has " + ObjExpansions.Count + " entries");
		if (DEBUG)
			foreach (var kvp in ObjExpansions)
				Logger.Debug($"\t{kvp.Key} -> {Util.Set2Str(kvp.Value, 3)}, ...");
		Logger.Debug("CmdIdentifiers has " + CmdIdentifiers.Count + " entries");
		if (DEBUG)
			foreach (var kvp in CmdIdentifiers)
				Logger.Debug($"\t{kvp.Key} -> {Util.Set2Str(kvp.Value, 3)}, ...");
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
