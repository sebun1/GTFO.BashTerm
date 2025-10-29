using LevelGeneration;

namespace BashTerm.Exec;

public abstract record PipeObject;

public record NullObject() : PipeObject;

public record ItemList(List<iTerminalItem> items) : PipeObject;

public record ItemQueryResult(
	bool Success,
	string ItemName,
	string Zone,
	bool Pingable,
	int Capacity
	) : PipeObject {
	public bool HasCapacity => Capacity > 0;
}

public record ItemQueryResults(List<ItemQueryResult> Results) : PipeObject;

