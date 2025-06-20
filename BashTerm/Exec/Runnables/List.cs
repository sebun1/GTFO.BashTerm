using LevelGeneration;

namespace BashTerm.Exec.Runnables;

[CommandHandler("list")]
public class List : IRunnable {
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

		// TODO: Not working on single argument

		// Il2CppSystem.Collections.Generic.Dictionary<string, iTerminalItem>
		var allTerminalInterfaces = LG_LevelInteractionManager.GetAllTerminalInterfaces();
		List<iTerminalItem> items = new List<iTerminalItem>();

		TerminalContext.WantToSendCommand(TERM_Command.ShowList, input, args[0], args.Count > 1 ? args[1] : "");

		foreach (var i in allTerminalInterfaces) {
			iTerminalItem item = i.Value;
			if (item.ShowInFloorInventory && SatisfiesFilters(item, args[0], args.Count > 1 ? args[1] : "", terminal)) {
				items.Add(item);
			}
		}

		return new ItemList(items);
	}

	private bool SatisfiesFilters(iTerminalItem item, string filter0, string filter1, LG_ComputerTerminal terminal) {
		if (item.SpawnNode == null || item.SpawnNode.m_dimension == null)
			return false;

		// Only consider items in the same dimension
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

}
