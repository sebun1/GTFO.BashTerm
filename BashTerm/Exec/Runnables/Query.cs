using System.Text.RegularExpressions;
using LevelGeneration;
using BashTerm.Parsers;
using BashTerm.Utils;

namespace BashTerm.Exec.Runnables;

[CommandHandler("query")]
public class Query : IRunnable {
	public string CommandName => "query";
	public string Desc => "Queries the location of a single item (or multiple through piping)";
	public string Manual => @"
NAME
		query - tool for querying the locations of items throughout the complex

USAGE
		QUERY <u>ITEM</u> -> <b>ItemQueryResult</b>|<b>ItemQueryResults</b>
		<b>ItemList</b> -> QUERY [-S <u>SORTING RULE</u>]

OPTIONS
		-S, --SORT
			Sort the list printed in the query summary in a specific order specified with a single sorting string, this also changes the order of items returned in ItemQueryResults.

			There are three categories for sorting that can be configured, each specified with a flag
				Item ID      I
				Zone         Z or L
				Capacity     C

			To specify whether you want a category to be specified in ascending or descending order, immediately follow the flag with a + or - e.g. ""I-"", if no order is specified, the sort defaults to ascending order (i.e. +).

			The priority for sorting is determined by the relative location of the flags in the string, if category x has its flag placed before category y, then query will try to sort via category x, if the results are inconclusive (i.e. there is a draw), it will fall back and compare category y, and so on.

			For example, the sorting string ""Z+I+C-"" asks query to sort by zone number first in ascending order, if that fails sort by the item ID in ascending order, then sort capacity in descending order (items with most capacity comes first). Taking default behavior into mind, this sorting string can also be equivalently written as ""ZIC-"".
";

	public FlagSchema FSchema { get; }

	public Query() {
		FSchema = new FlagSchema();
		FSchema.Add("s", "sort", FlagType.Value);
	}

	public PipedPayload Run(string cmd, List<string> args, CmdOpts opts, PipedPayload payload, LG_ComputerTerminal terminal) {
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);

		string input = Util.GetCommandString(cmd, args);

		switch (payload) {
			case ItemList(List<iTerminalItem> items):
				List<ItemQueryResult> results = new List<ItemQueryResult>();
				float timeCost = GetAdjustedQueryCost(items.Count);
				string timeCostStr = timeCost.ToString("N0");
				// TODO: Make default configurable in config
				string sortFlag = (opts["-s"] ?? "Z+I+C-").Trim().ToUpper();
				Logger.Debug($"Query cost: {timeCostStr}, priority flag: {sortFlag}");

				items.Sort(new TerminalItemComparator(sortFlag));

				terminal.m_command.AddOutput(TerminalLineType.SpinningWaitDone,
					$"Querying {items.Count} items (ETA: {timeCostStr}s)", timeCost);
				PrintQuerySummary(items, sortFlag, terminal);
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
				}
				return new ItemQueryResult(false, "", "ZONE_???", false, 0);
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

	private void PrintQuerySummary(List<iTerminalItem> items, string sortFlag, LG_ComputerTerminal terminal) {
		var lines = new Il2CppSystem.Collections.Generic.List<string>();
		List<short> col = new List<short> { 0, 25, 35 };
		terminal.m_command.AddOutput($"\n{Clr.Accent}<b>Query Summary</b>{Clr.End} ", spacing: false);
		terminal.m_command.AddOutput($"\n{Clr.Info}Sort=[{sortFlag}]{Clr.End}", spacing: false);

		string resHeader = "";
		resHeader += $"{Fmt.Pos(col[0])}ID {Clr.Info}[I]{Clr.End}{Fmt.EndPos}";
		resHeader += $"{Fmt.Pos(col[1])}CAPACITY {Clr.Info}[C]{Clr.End}{Fmt.EndPos}";
		resHeader += $"{Fmt.Pos(col[2])}LOCATION {Clr.Info}[Z/L]{Clr.End}{Fmt.EndPos}";
		lines.Add($"{resHeader}\n");

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

	public bool TryGetVarValue(LG_ComputerTerminal term, string varName, out string value) {
		value = "";
		return false;
	}

	public bool TryExpandArg(LG_ComputerTerminal term, string arg, out string expanded) {
		expanded = "";
		return false;
	}
}

internal class TerminalItemComparator : IComparer<iTerminalItem> {
	private readonly string _priorityFlag;

	public TerminalItemComparator(string priorityFlag) {
		_priorityFlag = priorityFlag.Trim().ToUpper();
	}

	public int Compare(iTerminalItem x, iTerminalItem y) {
		for (int i = 0; i < _priorityFlag.Length; i++) {
			char field = _priorityFlag[i];
			char direction = i + 1 < _priorityFlag.Length ? _priorityFlag[i + 1] : (char)0;

			int result = field switch {
				'I' => CompareIDNoNumber(x, y),
				'Z' or 'L' => CompareZone(x, y),
				'C' => CompareCapacity(x, y),
				_ => 0
			};

			if (result != 0)
				return direction == '-' ? -result : result;
		}
		// Logger.Debug($"Comparing {x.TerminalItemKey} to {y.TerminalItemKey}. CompareID={CompareIDNoNumber(x, y)} CompareZone={CompareZone(x, y)} CompareCapacity={CompareCapacity(x, y)}");

		return CompareID(x, y);
	}

	public int CompareCapacity(iTerminalItem x, iTerminalItem y) {
		return Query.GetCapacity(x).CompareTo(Query.GetCapacity(y));
	}

	public int CompareZone(iTerminalItem x, iTerminalItem y) {
		return x.SpawnNode.m_zone.ID.CompareTo(y.SpawnNode.m_zone.ID);
	}

	public int CompareIDNoNumber(iTerminalItem x, iTerminalItem y) {
		return string.CompareOrdinal(Util.RemoveAllNumbers(x.TerminalItemKey), Util.RemoveAllNumbers(y.TerminalItemKey));
	}

	public int CompareID(iTerminalItem x, iTerminalItem y) {
		return string.CompareOrdinal(x.TerminalItemKey, y.TerminalItemKey);
	}
}
