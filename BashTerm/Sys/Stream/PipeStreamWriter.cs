namespace BashTerm.Sys.Stream;

public sealed class PipeStreamWriter<T> {
	private PipeStream<T> _owner;
	public PipeStream<T> Owner => _owner;

	internal PipeStreamWriter(PipeStream<T> owner) {
		_owner = owner;
	}

	/// <summary>
	/// Tries to write an object to the stream
	/// </summary>
	/// <param name="item">item to write to the stream</param>
	/// <returns>true if the write was successful</returns>
	public bool TryWrite(T item) {
		return !_owner.IsCompleted && _owner.Enqueue(item);
	}

	public bool TryWriteMultiple(ICollection<T> items) {
		return !_owner.IsCompleted && _owner.EnqueueMultiple(items);
	}

	/// <summary>
	/// Set the writer to complete
	/// </summary>
	public void Complete() => _owner.Complete();

	/// <summary>
	/// True if the writer is completed
	/// i.e. no more data can be written
	/// </summary>
	public bool IsCompleted => _owner.IsCompleted;
}
