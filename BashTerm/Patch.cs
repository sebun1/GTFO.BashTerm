using HarmonyLib;
using LevelGeneration;

namespace BashTerm;

[HarmonyPatch]
internal class Patch {
	[HarmonyPatch(
		typeof(LG_ComputerTerminalCommandInterpreter),
		nameof(LG_ComputerTerminalCommandInterpreter.EvaluateInput)
		//nameof(LG_ComputerTerminalCommandInterpreter.TryGetCommand)
		)]
	[HarmonyPrefix]
	// TODO: Custom aliases defined via config
	public static void PreEvaluateCmd(ref string inputString) {
		string[] args = inputString.ToLower().Split(' ');
		string cmd = "";
		string param1 = "";
		string param2 = "";

		Logger.Info("Pre-Evaluate Command Received = \"" + inputString + "\"");

		if (args.Length == 0)
			return;
		cmd = args[0];
		if (args.Length > 1)
			param1 = args[1];
		if (args.Length > 2)
			param2 = args[2];

		switch (cmd) {
			case "l":
			case "ls":
				cmd = "list";
				break;
			case "lu":
			case "lsu":
				if (param2 == "") {
					cmd = "list";
					if (param1 != "" && int.TryParse(param1, out int n)) {
						param2 = "e_" + param1;
					} else
					{
						param2 = param1;
					}
					param1 = "u";
				}
				break;
			case "uc":
				cmd = "uplink_connect";
				break;
			case "rs":
			case "start":
				cmd = "reactor_startup";
				break;
			case "rsd":
			case "shut":
			case "shutdown":
				cmd = "reactor_shutdown";
				break;
			case "uv":
				if (ConfigMaster.EnableUVAlias)
                    cmd = "uplink_verify";
				break;
			case "rv":
				if (ConfigMaster.EnableRVAlias)
                    cmd = "reactor_verify";
				break;
			case "p":
			case "ping":
				// TODO: This logic is pretty limited, but works for now
				if (args.Length > 3 || 
					// below is considering the case they didn't executed a command with -T (e.g. ping ??? -t)
					(args.Length == 3 && !param1.StartsWith("-") && Util.IsInt(param2))) {
                    param1 = Util.InterpretObjectType(args[1]) + "_" + Util.Concat('_', args, 2, args.Length);
                    param2 = "";
				}				
				break;
			case "q":
			case "query":
				if (args.Length > 2) { 
                    param1 = Util.InterpretObjectType(args[1]) + "_" + Util.Concat('_', args, 2, args.Length);
                    param2 = "";
				}
				break;
			case "clear":
				cmd = "cls";
				break;
		}

		if (cmd == "raw" || cmd == "r") {
			inputString = Util.Concat(args, 1, args.Length);
		} else {
            inputString = Util.Concat(cmd, param1, param2).ToUpper().Trim();
		}

		Logger.Info("Command Evaluated as \"" + inputString + "\"");
	}
}
