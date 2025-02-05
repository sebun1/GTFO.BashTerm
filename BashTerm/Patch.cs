using HarmonyLib;
using LevelGeneration;
using System.Reflection.Metadata.Ecma335;

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

	// Patch TODO: Better autocomplete -- list out candidates

	// Patch TODO: Better prompt -- shows zone, etc, zsh style

	// Patch TODO: Add more advanced filter options (e.g. zone 115|116|118 -> zone 115 or 116 or 118)

	// Patch TODO: Preserves the command typed?

	// Patch TODO: Ctrl + <BS> clears until the next whitespace instead of the
	// whitespace as well (e.g. "cmd arg1" -> "cmd ")

	// Patch TODO: Shows some current objective information needed in terminal (e.g. uplink address 192.168.1.1)

	// Patch TODO: More detailed help for specific commands

	// Possible TODO: Terminal effect in multiplayer lobby? Does it sync?
}
