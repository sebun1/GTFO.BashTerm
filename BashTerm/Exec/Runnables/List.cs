using BashTerm.Parsers;
using BashTerm.Utils;
using LevelGeneration;

namespace BashTerm.Exec.Runnables;

[CommandHandler("list")]
public class List : IRunnable {
	public string CommandName => "list";
	public string Desc => "";

	public string Manual => @"
Usage: list [OPTIONS] FILTER_1, FILTER_2

Options:
	N/A
";

	public FlagSchema FSchema { get; }

	public List() {
		FSchema = new FlagSchema();
	}

	public PipedPayload Run(string cmd, List<string> args, CmdOpts opts, PipedPayload payload, LG_ComputerTerminal terminal) {
		if (terminal == null) throw new NullTerminalInstanceException(CommandName);
		string input = Util.GetCommandString(cmd, args);

		var arg0 = args.Count > 0 ? args[0] : "";
		var arg1 = args.Count > 1 ? args[1] : "";

		var allTerminalInterfaces = LG_LevelInteractionManager.GetAllTerminalInterfaces();
		List<iTerminalItem> items = new List<iTerminalItem>();

		LG_ComputerTerminalManager.WantToSendTerminalCommand(terminal.SyncID, TERM_Command.ShowList, input, arg0, arg1);

		foreach (var i in allTerminalInterfaces) {
			iTerminalItem item = i.Value;
			if (item.ShowInFloorInventory && SatisfiesFilters(item, arg0, arg1, terminal)) {
				items.Add(item);
			}
		}

		return new ItemList(items);
	}

	private bool SatisfiesFilters(iTerminalItem item, string filter0, string filter1, LG_ComputerTerminal terminal) {
		if (item.SpawnNode == null || item.SpawnNode.m_dimension == null)
			return false;

		if (item.SpawnNode.m_dimension.DimensionIndex != terminal.SpawnNode.m_dimension.DimensionIndex)
			return false;

		string searchableText = string.Concat(
			item.TerminalItemKey, " ",
			item.FloorItemType, " ",
			item.FloorItemStatus, " ",
			item.FloorItemLocation, " ",
			eFloorInventoryObjectBeaconStatus.NoBeacon.ToString()
		).ToUpper();

		bool usingFirstFilter = !string.IsNullOrWhiteSpace(filter0);
		bool usingSecondFilter = !string.IsNullOrWhiteSpace(filter1);

		bool matchesFirst = usingFirstFilter && searchableText.Contains(filter0.ToUpper());
		bool matchesSecond = usingSecondFilter && searchableText.Contains(filter1.ToUpper());

		bool noFilters = !usingFirstFilter && !usingSecondFilter;
		bool satisfiesFilters = (!usingFirstFilter || matchesFirst) && (!usingSecondFilter || matchesSecond);

		return noFilters || satisfiesFilters;
	}

	public bool TryGetVarValue(LG_ComputerTerminal term, string varName, out string value) {
		value = "";
		return false;
	}

	public bool TryExpandArg(LG_ComputerTerminal term, string arg, out string expanded) {
		if (ConfigMgr.LsConvertsNum2ZoneId && Util.IsInt(arg)) {
			expanded = $"e_{arg}";
			return true;
		}
		if (ConfigMgr.LsObjExpansions && ParseUtil.TryExpandObj(arg, out expanded))
			return true;
		expanded = arg;
		return false;
	}
}
