using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Bsh.Sys.Stream;

public enum OverflowMode {
	DropOldest,
	DropNewest
}

public sealed class PipeStream<T> : IDisposable {
	private readonly ConcurrentQueue<T> _q = new();
	private readonly int _capacity;
	private readonly OverflowMode _overflow;
	private readonly bool _uncapped;

	private volatile bool _writerCreated, _readerCreated, _completed;

	// atomic count to avoid O(n) ConcurrentQueue.Count calls
	private int _count;

	/// <summary>
	/// Creates a new PipeStream with the specified capacity and overflow mode.
	/// </summary>
	/// <param name="capacity">capacity of the stream, 0 for uncapped</param>
	/// <param name="overflow">how to manage overflowing items</param>
	public PipeStream(int capacity = 0, OverflowMode overflow = OverflowMode.DropOldest) {
		_uncapped = capacity <= 0;
		_capacity = _uncapped ? -1 : capacity;
		_overflow = overflow;
		_count = 0;
	}

	/// <summary>
	/// Creates a writer for this stream
	/// </summary>
	/// <returns>instance of PipeStreamWriter</returns>
	/// <exception cref="InvalidOperationException">if a writer was already created</exception>
	public PipeStreamWriter<T> CreateWriter() {
		if (_writerCreated) throw new InvalidOperationException("Writer already created.");
		_writerCreated = true;
		return new PipeStreamWriter<T>(this);
	}

	/// <summary>
	/// Creates a reader for this stream.
	/// </summary>
	/// <returns>instance of PipeStreamReader</returns>
	/// <exception cref="InvalidOperationException">if a reader was already created</exception>
	public PipeStreamReader<T> CreateReader() {
		if (_readerCreated) throw new InvalidOperationException("Reader already created.");
		_readerCreated = true;
		return new PipeStreamReader<T>(this);
	}

	/// <summary>
	/// Enqueues an item to the stream.
	/// In case of overflow, removes oldest items if OverflowMode is DropOldest, otherwise drops the new item.
	/// </summary>
	/// <param name="item">item to be enqueued</param>
	/// <returns>true if the item is enqueued</returns>
	internal bool Enqueue(T item) {
		if (_completed) return false;

		// If capped, ensure there's room. Use atomic _count for O(1) checks.
		while (!_uncapped && Volatile.Read(ref _count) >= _capacity) {
			if (_overflow == OverflowMode.DropNewest) return false;
			// Drop oldest
			if (_q.TryDequeue(out _)) {
				Interlocked.Decrement(ref _count);
			} else {
				// Nothing to drop, break to avoid spinning
				break;
			}
		}

		_q.Enqueue(item);
		Interlocked.Increment(ref _count);
		return true;
	}

	/// <summary>
	/// Enqueues multiple items to the stream, does nothing if capacity would be exceeded
	/// </summary>
	/// <param name="items">items to be enqueued</param>
	/// <returns>true if items are successfully queued, otherwise false</returns>
	internal bool EnqueueMultiple(ICollection<T> items) {
		if (_completed) return false;
		int itemsCount = items.Count; // use ICollection<T>.Count property (O(1))

		if (!_uncapped && Volatile.Read(ref _count) + itemsCount > _capacity) return false;

		foreach (T item in items) {
			_q.Enqueue(item);
		}

		Interlocked.Add(ref _count, itemsCount);
		return true;
	}

	/// <summary>
	/// Dequeues a batch of items from the stream into the provided buffer
	/// </summary>
	/// <param name="buffer">buffer to dequeue to</param>
	/// <returns>number of items dequeued to buffer</returns>
	internal int TryDequeueBatch(Span<T> buffer) {
		int n = 0;
		while (n < buffer.Length && _q.TryDequeue(out var x)) {
			buffer[n++] = x;
			Interlocked.Decrement(ref _count);
		}

		return n;
	}

	/// <summary>
	/// Dequeues all available items into the provided buffer
	/// </summary>
	/// <param name="buffer">buffer to dequeue into</param>
	/// <param name="itemsDequeued">number of items dequeued</param>
	/// <returns>true when buffer is of sufficient size</returns>
	internal bool DequeueAll(Span<T> buffer, [NotNullWhen(true)] out int? itemsDequeued) {
		if (_count > buffer.Length) {
			itemsDequeued = null;
			return false;
		}

		int n = 0;
		while (n < buffer.Length && _q.TryDequeue(out var x)) {
			buffer[n++] = x;
			Interlocked.Decrement(ref _count);
		}

		itemsDequeued = n;
		return true;
	}

	/// <summary>
	/// Tries to dequeue specific number of items from the stream, fails if not enough items are available
	/// </summary>
	/// <param name="count">number of items to dequeue</param>
	/// <param name="items">items dequeued if successful</param>
	/// <returns>true if operation was successful</returns>
	internal bool TryDequeueMultiple(int count, Span<T> items) {
		// Validate arguments
		if (count <= 0 || items.Length < count)
			return false;
		// Fast fail if not enough items at the moment of call
		if (Volatile.Read(ref _count) < count) return false;

		int n = 0;
		while (n < count && _q.TryDequeue(out var x)) {
			items[n++] = x;
			Interlocked.Decrement(ref _count);
		}

		// Succeed only if requested number were dequeued
		return n == count;
	}

	/// <summary>
	/// Tries to dequeue an item from the stream
	/// </summary>
	/// <param name="item">item dequeued if successful</param>
	/// <returns>true if dequeue was successful</returns>
	internal bool TryDequeue(out T? item) {
		if (_q.TryDequeue(out item)) {
			Interlocked.Decrement(ref _count);
			return true;
		}

		return false;
	}

	/// <summary>
	/// True if the stream is empty
	/// </summary>
	public bool IsEmpty => Volatile.Read(ref _count) == 0;

	/// <summary>
	/// True if the stream is uncapped
	/// i.e. has no capacity limit
	/// </summary>
	public bool IsUncapped => _uncapped;

	/// <summary>
	/// The capacity of the stream, -1 if uncapped
	/// </summary>
	public int Capacity => _capacity;

	internal ConcurrentQueue<T> Queue => _q;

	internal int Count => Volatile.Read(ref _count);

	/// <summary>
	/// True if the stream is completed
	/// </summary>
	public bool IsCompleted => _completed;

	/// <summary>
	/// Mark the stream as completed
	/// </summary>
	internal void Complete() => _completed = true;

	/// <summary>
	/// Mark the stream as disposed, completed
	/// </summary>
	public void Dispose() {
		_completed = true;
	}

	public bool Peek(out T? item) {
		return _q.TryPeek(out item);
	}
}
