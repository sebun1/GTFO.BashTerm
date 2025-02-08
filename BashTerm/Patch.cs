using Dissonance;
using HarmonyLib;
using LevelGeneration;

namespace BashTerm;

[HarmonyPatch]
internal class Patch {
	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.EvaluateInput)
	)]
	[HarmonyPrefix]
	public static void PreEvaluateCmd(ref string inputString) {
		string[] args = ParseUtil.CleanSplit(inputString.ToLower());
		string cmd;
		string param1 = "";
		string param2 = "";

		Logger.Debug("Pre-Evaluate Command Received = \"" + inputString + "\"");

		if (args.Length == 0)
			return;
		cmd = args[0];
		if (args.Length > 1)
			param1 = args[1];
		if (args.Length > 2)
			param2 = args[2];

		string cmdExpansion = ParseUtil.GetCmdExpansion(cmd);
		TermCmd cmdType = ParseUtil.GetCmdType(cmdExpansion);

		Util.printMaps();

		Logger.Debug("PreEval: cmdExpression = " + cmdExpansion);
		Logger.Debug("PreEval: cmdType = " + cmdType.ToString("G"));

		bool doExpansion = true;

		switch (cmdType) {
			case TermCmd.ShowList:
				// TODO: Consider the LSU expansion v.s. LIST case
				if (ParseUtil.GetCmdExpansion("lsu") == cmdExpansion) {
					if (Util.IsInt(param1)) param1 = "e_" + param1;
				} else {
					bool firstIsInt = Util.IsInt(param1);
					bool secondIsInt = Util.IsInt(param2);

					if (ConfigMaster.LsConvertsNum2ZoneId) {
						if (firstIsInt) {
							param1 = "e_" + param1;
						} else if (secondIsInt) {
							param2 = "e_" + param2;
						}
					}

					if (ConfigMaster.LsObjExpansions) {
						if (!string.IsNullOrWhiteSpace(param1) && !firstIsInt) {
							param1 = ParseUtil.GetObjExpansion(param1);
						} else if (!string.IsNullOrWhiteSpace(param2) && !secondIsInt) {
							param2 = ParseUtil.GetObjExpansion(param2);
						}
					}
				}
				break;
			case TermCmd.TerminalUplinkVerify:
				doExpansion = ConfigMaster.EnableUVAlias;
				break;
			case TermCmd.ReactorVerify:
				doExpansion = ConfigMaster.EnableRVAlias;
				break;
			case TermCmd.Ping:
				if (
					args.Length > 3
					// TODO: Double check logic on this one, does this make sense? It doesn't transform if two args and none of them are numbers
					|| (args.Length == 3 && !Util.ContainsFlag(args) && Util.IsInt(param2))
				) {
					param1 =
						ParseUtil.GetObjExpansion(args[1])
						+ "_"
						+ Util.Concat('_', args, 2, args.Length);
					param2 = "";
				}
				break;
			case TermCmd.Query:
				if (args.Length > 2) {
					param1 =
						ParseUtil.GetObjExpansion(args[1])
						+ "_"
						+ Util.Concat('_', args, 2, args.Length);
					param2 = "";
				}
				break;
			case TermCmd.Raw:
				inputString = Util.Concat(args, 1, args.Length);
				inputString = inputString.ToUpper().Trim(); // Always uppercase and trim before releasing
				return;
		}

		if (doExpansion) {
			Logger.Debug("Doing Expansion");
			cmd = cmdExpansion;
		}

		Logger.Debug("cmd = " + cmd);
		Logger.Debug("param1 = " + param1);
		Logger.Debug("param2 = " + param2);

		inputString = Util.Concat(cmd, param1, param2);

		inputString = inputString.ToUpper().Trim(); // Always uppercase and trim before releasing

		Logger.Debug("Command Evaluated as \"" + inputString + "\"");
	}
}
