namespace Bsh.Sys.Stream;

public sealed class PipeStreamReader<T> {
	private PipeStream<T> _owner;
	public PipeStream<T> Owner => _owner;

	internal PipeStreamReader(PipeStream<T> owner) {
		_owner = owner;
	}

	/// <summary>
	/// Drains up to maxItems number of items in the stream and
	/// performs onItem action on them
	/// </summary>
	/// <param name="maxItems">maximum number of items drained from stream</param>
	/// <param name="onItem">function to execute on each item</param>
	/// <returns>number of actions performed/items drained</returns>
	public int Drain(int maxItems, Action<T> onItem) {
		int drained = 0;
		const int chunk = 64;
		var tmp = new T[Math.Min(chunk, maxItems)];
		while (drained < maxItems) {
			int toRead = Math.Min(tmp.Length, maxItems - drained);
			int got = _owner.TryDequeueBatch(tmp.AsSpan(0, toRead));
			if (got == 0) break;
			for (int i = 0; i < got; i++) onItem(tmp[i]);
			drained += got;
		}

		return drained;
	}

	public List<T> ReadAll() {
		T[] arr = new T[_owner.Count];
		Span<T> buff = arr.AsSpan();
		if (!_owner.DequeueAll(buff, out var read))
			throw new WTFException(
				"PipeStreamReader.ReadAll tried to read with buffer size of owner queue" +
				"count but is somehow insufficient to read all items.");
		int r = read.Value;
		List<T> list = new List<T>(r);
		for (int i = 0; i < r; i++)
			list.Add(arr[i]);
		return list;
	}

	public bool Peek(out T? item) => _owner.Peek(out item);

	/// <summary>
	/// Tries to dequeue specific number of items from the stream, fails if not enough items are available
	/// </summary>
	/// <param name="count">number of items to dequeue</param>
	/// <param name="items">caller-provided buffer to receive items</param>
	/// <returns>true if operation was successful</returns>
	public bool TryReadMultiple(int count, Span<T> items) => _owner.TryDequeueMultiple(count, items);

	/// <summary>
	/// Tries to read a single item from the stream
	/// </summary>
	/// <param name="item">returning item</param>
	/// <returns>true if the read was successful</returns>
	public bool TryRead(out T? item) => _owner.TryDequeue(out item);

	/// <summary>
	/// True if there are items available to read
	/// </summary>
	public bool Available => !_owner.IsEmpty;

	/// <summary>
	/// True if the stream is closed (completed and empty)
	/// </summary>
	public bool IsClosed => _owner is { IsCompleted: true, IsEmpty: true };
}
