using LevelGeneration;

namespace BashTerm.Exec;

public abstract record PipedPayload;

public record EmptyPayload() : PipedPayload;

public record StringOutput(string String) : PipedPayload;

public record ItemList(List<iTerminalItem> items) : PipedPayload;

public record ItemQueryResult(
	bool Success,
	string ItemName,
	string Zone,
	bool Pingable,
	int Capacity
	) : PipedPayload {
	public bool HasCapacity => Capacity > 0;
}

public record ItemQueryResults(List<ItemQueryResult> Results) : PipedPayload;

