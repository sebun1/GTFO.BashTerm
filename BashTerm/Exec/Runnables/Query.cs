using System.Text.RegularExpressions;
using Gear;
using LevelGeneration;

namespace BashTerm.Exec.Runnables;

[CommandHandler("query")]
public class Query : IRunnable {
	public string CommandName => "list";
	public string Description => "";
	public string Help => "";

	public PipedPayload Run(string cmd, List<string> args, PipedPayload payload, LG_ComputerTerminal? termInherit) {
		var terminal = termInherit == null ? TerminalContext.TerminalInstance : termInherit;
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);
		string input = cmd;
		if (args.Count > 0)
			input += " " + string.Join(" ", args);
		input = input.ToUpper();

		switch (payload) {
			case ItemList(List<iTerminalItem> items):
				List<ItemQueryResult> results = new List<ItemQueryResult>();
				int timeCost = 3 * items.Count;
				terminal.m_command.AddOutput(TerminalLineType.SpinningWaitDone,
					$"Querying {items.Count} items (ETA: {timeCost}s)", (float)(timeCost)); // TODO: Log Time Scale
				PrintQuerySummary(items, terminal);
				foreach (var item in items) {
					results.Add(new ItemQueryResult(
						true,
						item.TerminalItemKey,
						item.FloorItemLocation,
						item.SpawnNode != null && terminal.SpawnNode != null &&
						terminal.SpawnNode.m_zone == item.SpawnNode.m_zone,
						GetCapacity(item)
					));
				}
				return new ItemQueryResults(results);

			default:
				if (args.Count > 1)
					throw new TooManyArgumentsException(CommandName, args.Count, 1);
				if (args.Count == 0)
					throw new MissingArgumentException(CommandName, 0, 1);
				LG_ComputerTerminalManager.WantToSendTerminalCommand(terminal.SyncID, TERM_Command.Query, input,
					args[0], "");

				if (LG_LevelInteractionManager.TryGetTerminalInterface(args[0].ToUpper(),
					    terminal.SpawnNode.m_dimension.DimensionIndex, out var target)) {
					return new ItemQueryResult(
						true,
						target.TerminalItemKey,
						target.FloorItemLocation,
						target.SpawnNode != null && terminal.SpawnNode != null &&
						terminal.SpawnNode.m_zone == target.SpawnNode.m_zone,
						GetCapacity(target)
					);
				} else {
					return new ItemQueryResult(false, "", "ZONE_???", false, 0);
				}
		}
	}

	private int GetCapacity(iTerminalItem item) {
		var infoList = item.GetDetailedInfo(new Il2CppSystem.Collections.Generic.List<string>());
		if (infoList.Count < 3) return -1;
		var match = Regex.Match(infoList[2], @"CAPACITY:\s*(\d+)%");
		if (match.Success && int.TryParse(match.Groups[1].Value, out int cap)) {
			return cap;
		} else {
			return -1;
		}
	}

	private void PrintQuerySummary(List<iTerminalItem> items, LG_ComputerTerminal terminal) {
		var lines = new Il2CppSystem.Collections.Generic.List<string>();
		List<short> col = new List<short> { 0, 25, 50 };
		terminal.m_command.AddOutput(
			$"{Fmt.Pos(col[0])}ID{Fmt.EndPos}{Fmt.Pos(col[1])}LOCATION{Fmt.EndPos}{Fmt.Pos(col[2])}CAPACITY{Fmt.EndPos}");
		foreach (var item in items) {
			int cap = GetCapacity(item);
			string capString = cap < 0 ? "N/A" : cap.ToString() + "%";
			string str = $"{Fmt.Pos(col[0])}{item.TerminalItemKey}{Fmt.EndPos}";
			str += $"{Fmt.Pos(col[1])}{item.FloorItemLocation}{Fmt.EndPos}";
			str += $"{Fmt.Pos(col[2])}{capString}{Fmt.EndPos}";
			lines.Add(str);
		}

		terminal.m_command.AddOutput(lines);
	}
}
