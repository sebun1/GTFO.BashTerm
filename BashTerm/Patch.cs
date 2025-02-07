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
	// TODO: Custom aliases defined via config
	public static void PreEvaluateCmd(ref string inputString) {
		string[] args = inputString.ToLower().Split(' ');
		string cmd = "";
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

		TermCmd cmdInternal = Util.InterpretCmd(cmd);
		bool doTranslate = true;

		switch (cmdInternal) {
			case TermCmd.ShowList:
				if (ConfigMaster.LsConvertsNum2ZoneId) {
					bool firstIsInt = Util.IsInt(param1);
					bool secondIsInt = Util.IsInt(param2);

					if (firstIsInt) {
						param1 = "e_" + param1;
						if (!secondIsInt && ConfigMaster.LsObjExpansions) {
							param2 = Util.InterpretObject(param2);
						}
					} else {
						if (secondIsInt)
							param2 = "e_" + param2;
						if (ConfigMaster.LsObjExpansions)
							param1 = Util.InterpretObject(param1);
					}
				}
				break;
			case TermCmd.ShowListU:
				param2 = "";
				if (param2 == "") {
					cmd = "list";
					if (param1 != "" && int.TryParse(param1, out int n)) {
						param2 = "e_" + param1;
					} else {
						param2 = param1;
					}
					param1 = "u";
				}
				break;
			case TermCmd.TerminalUplinkVerify:
				doTranslate = ConfigMaster.EnableUVAlias;
				break;
			case TermCmd.ReactorVerify:
				doTranslate = ConfigMaster.EnableRVAlias;
				break;
			case TermCmd.Ping:
				cmd = "ping";
				if (
					args.Length > 3
					|| (args.Length == 3 && !Util.ContainsFlag(args) && Util.IsInt(param2))
				) {
					param1 =
						Util.InterpretObject(args[1])
						+ "_"
						+ Util.Concat("_", args, 2, args.Length);
					param2 = "";
				}
				break;
			case TermCmd.Query:
				cmd = "query";
				if (args.Length > 2) {
					param1 =
						Util.InterpretObject(args[1])
						+ "_"
						+ Util.Concat("_", args, 2, args.Length);
					param2 = "";
				}
				break;
			case TermCmd.Raw:
				inputString = Util.Concat(args, 1, args.Length);
				return;
				/*
				case TermCmd.TerminalUplinkConnect:
					break;
				case TermCmd.ReactorStartup:
					break;
				case TermCmd.ReactorShutdown:
					break;
				case TermCmd.ReadLog:
					break;
				case TermCmd.Cls:
					break;
				*/
		}

		Logger.Debug("We interpreted cmdInternal = " + cmdInternal.ToString("G"));

		if (cmdInternal != TermCmd.None && doTranslate) {
			cmd = Util.Cmd2Txt(cmdInternal);
			Logger.Debug("Command is not None and we are translating, yay! to: " + cmd);
		}

		inputString = Util.Concat(cmd, param1, param2);

		inputString = inputString.ToUpper().Trim();

		Logger.Debug("Command Evaluated as \"" + inputString + "\"");
	}
}
