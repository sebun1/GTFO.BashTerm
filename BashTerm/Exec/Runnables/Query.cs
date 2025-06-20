using System.Text.RegularExpressions;
using Gear;
using LevelGeneration;
using BashTerm;
using BashTerm.Parsers;
using Dissonance;

namespace BashTerm.Exec.Runnables;

[CommandHandler("query")]
public class Query : IRunnable {
	public string CommandName => "query";
	public string Desc => "Queries the location of a single item (or multiple through piping)";
	public string Man => "Usage";

	public PipedPayload Run(string cmd, List<string> args, PipedPayload payload, LG_ComputerTerminal terminal) {
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);

		string input = Util.GetCommandString(cmd, args);
		FlagParser fp = new FlagParser(args);

		switch (payload) {
			case ItemList(List<iTerminalItem> items):
				List<ItemQueryResult> results = new List<ItemQueryResult>();
				float timeCost = GetAdjustedQueryCost(items.Count);
				string timeCostStr = timeCost.ToString("N0");
				string priorityFlag = fp.GetVal("-s", "--sort") ?? "Z+I+Q-";
				Logger.Debug($"Query cost: {timeCostStr}, priority flag: {priorityFlag}");

				terminal.m_command.AddOutput(TerminalLineType.SpinningWaitDone,
					$"Querying {items.Count} items (ETA: {timeCostStr}s)", timeCost);
				PrintQuerySummary(items, terminal);
				items.Sort(new TerminalItemComparator(priorityFlag)); // TODO: Fix sorting, not working
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

	internal static int GetCapacity(iTerminalItem item) {
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
		terminal.m_command.AddOutput($"\n{Clr.Accent}<b>Query Summary</b>{Clr.End}", spacing: false);
		lines.Add($"{Clr.Purple}{Fmt.Pos(col[0])}ID{Fmt.EndPos}{Fmt.Pos(col[1])}CAPACITY{Fmt.EndPos}{Fmt.Pos(col[2])}LOCATION{Fmt.EndPos}{Clr.End}\n");
		foreach (var item in items) {
			int cap = GetCapacity(item);
			string capString = cap < 0 ? "-" : cap.ToString() + "%";
			string str = "";
			str += $"{Fmt.Pos(col[0])}{item.TerminalItemKey}{Fmt.EndPos}";
			str += $"{Fmt.Pos(col[1])}{capString}{Fmt.EndPos}";
			str += $"{Fmt.Pos(col[2])}{item.FloorItemLocation}{Fmt.EndPos}";
			lines.Add(str);
		}
		terminal.m_command.AddOutput(lines);
	}

	private float GetAdjustedQueryCost(int cost) {
		if (cost <= 0) return 0;
		float b = 0.4f;
		float c = 1.2f;
		return (float)Math.Round(cost / Math.Log(b * cost + c));
	}
}

internal class TerminalItemComparator : IComparer<iTerminalItem> {
	// Consisting of I Z C and +/- e.g. "I+Z-C-" "IZ+C-"
	private readonly string _priorityFlag;

	public TerminalItemComparator(string priorityFlag) {
		_priorityFlag = priorityFlag.Trim().ToUpper();
	}

	public int Compare(iTerminalItem x, iTerminalItem y) {
		for (int i = 0; i < _priorityFlag.Length - 1; i += 2) {
			char field = _priorityFlag[i];
			char direction = _priorityFlag[i + 1];

			int result = field switch {
				'I' => CompareID(x, y),
				'Z' => CompareZone(x, y),
				'C' => CompareCapacity(x, y),
				_ => 0
			};

			if (result != 0)
				return direction == '-' ? -result : result;
		}
		Logger.Debug($"Comparing {x.TerminalItemKey} to {y.TerminalItemKey}. CompareID={CompareID(x, y)} CompareZone={CompareZone(x, y)} CompareCapacity={CompareCapacity(x, y)}");

		return 0;
	}

	public int CompareCapacity(iTerminalItem x, iTerminalItem y) { // flag C
		return Query.GetCapacity(x).CompareTo(Query.GetCapacity(y));
	}

	public int CompareZone(iTerminalItem x, iTerminalItem y) { // flag Z
		return x.SpawnNode.m_zone.ID.CompareTo(y.SpawnNode.m_zone.ID);
	}

	public int CompareID(iTerminalItem x, iTerminalItem y) { // flag I
		return string.CompareOrdinal(x.TerminalItemKey, y.TerminalItemKey);
	}
}
