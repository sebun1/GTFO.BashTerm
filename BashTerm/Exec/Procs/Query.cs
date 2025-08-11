using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using LevelGeneration;
using BashTerm.Parsers;
using BashTerm.Sys;
using BashTerm.Utils;

namespace BashTerm.Exec.Procs;

[BshProc("query")]
public class Query : Proc {
	private const string Name = "query";
	private const string Desc = "Queries the location of a items";
	private const string Manual = @"
<b>NAME</b>
		query - tool for querying the locations of items throughout the complex

<b>USAGE</b>
		query <u>item</u> -> <b>ItemQueryResult</b>|<b>ItemQueryResults</b>
		<b>ItemList</b> -> query [-s <u>sorting string</u>]

<b>OPTIONS</b>
		-s, --sort
			Sort the list printed in the query summary in a specific order specified with a single sorting string, this also changes the order of items returned in ItemQueryResults.

			There are three categories for sorting that can be configured, each specified with a flag (case-insensitive):
				Item ID      I
				Zone         Z or L
				Capacity     C

			To specify whether you want a category to be specified in ascending or descending order, immediately follow the flag with a + or - e.g. ""I-"", if no order is specified, the sort defaults to ascending order (i.e. +).

			The priority for sorting is determined by the relative location of the flags in the string, if category x has its flag placed before category y, then query will try to sort via category x, if the results are inconclusive (i.e. there is a draw), it will fall back and compare category y, and so on.

			For example, the sorting string ""Z+I+C-"" asks query to sort by zone number first in ascending order, if that fails sort by the item ID in ascending order, then sort capacity in descending order (items with most capacity comes first). Taking default behavior into mind, this sorting string can also be equivalently written as ""ZIC-"".
";

	private static readonly bool RequestAlternateBuffer = false;

	private static readonly FlagSchema FSchema = CreateFlagSchema();

	private static FlagSchema CreateFlagSchema() {
		FlagSchema fs = new FlagSchema();
		fs.Add("s", "sort", FlagType.Value);
		return fs;
	}

	public static ProcManifest GetManifest() {
		return new ProcManifest(Name, Desc, Manual, RequestAlternateBuffer, FSchema);
	}

	//public PipedPayload Run(string cmd, List<string> args, CmdOpts opts, PipedPayload payload, LG_ComputerTerminal terminal) {
	public override void Start(StartPayload payload, LG_ComputerTerminal term) {
		if (term == null) throw new NullTerminalInstanceException(Name);
		ExitPayload ePayload = new();

		string input = Util.GetCommandString(Name, payload.Args);

		switch (payload.Payload) {
			case ItemList(List<iTerminalItem> items):
				List<ItemQueryResult> results = new List<ItemQueryResult>();
				float timeCost = GetAdjustedQueryCost(items.Count);
				string timeCostStr = timeCost.ToString("N0");
				// TODO: Make default configurable in config
				string sortFlag = (payload.Opts["-s"] ?? "Z+I+C-").Trim().ToUpper();
				Logr.Debug($"Query cost: {timeCostStr}, priority flag: {sortFlag}");

				items.Sort(new TerminalItemComparator(sortFlag));

				term.m_command.AddOutput(TerminalLineType.SpinningWaitDone,
					$"Querying {items.Count} items (ETA: {timeCostStr}s)", timeCost);
				PrintQuerySummary(items, sortFlag, term);
				foreach (var item in items) {
					results.Add(new ItemQueryResult(
						true,
						item.TerminalItemKey,
						item.FloorItemLocation,
						item.SpawnNode != null && term.SpawnNode != null &&
						term.SpawnNode.m_zone == item.SpawnNode.m_zone,
						GetCapacity(item)
					));
				}

				Exit(new ExitPayload(new ItemQueryResults(results)));
				return;
			default:
				if (payload.Args.Count == 0)
					throw new MissingArgumentException(Name, 0, 1);
				string objName = string.Join('_', payload.Args);
				LG_ComputerTerminalManager.WantToSendTerminalCommand(term.SyncID, TERM_Command.Query, input,
					objName, "");

				if (LG_LevelInteractionManager.TryGetTerminalInterface(payload.Args[0].ToUpper(),
					    term.SpawnNode.m_dimension.DimensionIndex, out var target)) {
					Exit(new ExitPayload(new ItemQueryResult(
						true,
						target.TerminalItemKey,
						target.FloorItemLocation,
						target.SpawnNode != null && term.SpawnNode != null &&
						term.SpawnNode.m_zone == target.SpawnNode.m_zone,
						GetCapacity(target)
					)));
					return;
				}
				Exit(new ExitPayload(-1, "The item is not pingable", new ItemQueryResult(false, "", "ZONE_???", false, 0)));
				return;
		}
	}

	public override void Update(UpdatePayload _) {
		Exit(new ExitPayload());
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
		terminal.m_command.AddOutput($"\n{Styles.C_Accent}<b>Query Summary</b>{Styles.C_End} ", spacing: false);
		terminal.m_command.AddOutput($"\n{Styles.C_Info}Sort=[{sortFlag}]{Styles.C_End}", spacing: false);

		string resHeader = "";
		resHeader += $"{Styles.Pos(col[0])}ID {Styles.C_Info}[I]{Styles.C_End}{Styles.EndPos}";
		resHeader += $"{Styles.Pos(col[1])}CAPACITY {Styles.C_Info}[C]{Styles.C_End}{Styles.EndPos}";
		resHeader += $"{Styles.Pos(col[2])}LOCATION {Styles.C_Info}[Z/L]{Styles.C_End}{Styles.EndPos}";
		lines.Add($"{resHeader}\n");

		foreach (var item in items) {
			int cap = GetCapacity(item);
			string capString = cap < 0 ? "-" : cap.ToString() + "%";
			string str = "";
			str += $"{Styles.Pos(col[0])}{item.TerminalItemKey}{Styles.EndPos}";
			str += $"{Styles.Pos(col[1])}{capString}{Styles.EndPos}";
			str += $"{Styles.Pos(col[2])}{item.FloorItemLocation}{Styles.EndPos}";
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
		if (ParseUtil.TryExpandObj(arg, out expanded))
			return true;
		return false;
	}
}

internal class TerminalItemComparator : IComparer<iTerminalItem> {
	private readonly string _priorityFlag;

	public TerminalItemComparator(string priorityFlag) {
		_priorityFlag = priorityFlag.Trim().ToUpper();
	}

	public int Compare(iTerminalItem? x, iTerminalItem? y) {
		if (x == null && y == null) return 0;
		if (x == null) return 1;
		if (y == null) return -1;

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
