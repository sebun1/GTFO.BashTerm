using System.Collections.Concurrent;
using BashTerm.Utils;

namespace BashTerm.Exec;

public static class Sync {
	private static ConcurrentDictionary<SyncSrc, ConcurrentQueue<TaskCompletionSource<bool>>> _channels = new();

	private static ConcurrentQueue<TaskCompletionSource<bool>> GetQueue(SyncSrc type) {
		return _channels.GetOrAdd(type, new ConcurrentQueue<TaskCompletionSource<bool>>());
	}

	public static void WaitFor(SyncSrc source, int timeoutMs) {
		Task.Run(async () => { await WaitAsync(source, timeoutMs); });
	}

	public static async Task WaitAsync(SyncSrc source, int timeoutMs) {
		Logger.Debug($"Sync.WaitAsync: waiting on {source} with timeout={timeoutMs}ms");
		var tcs = new TaskCompletionSource<bool>();
		var q = GetQueue(source);
		q.Enqueue(tcs);

		if (timeoutMs > 0) {
			var timeoutTask = Task.Delay(timeoutMs);
			var first = await Task.WhenAny(tcs.Task, timeoutTask);

			if (first == timeoutTask) {
				q.TryDequeue(out _);
				throw new TimeoutException($"channel {source} timed out");
			}
		} else {
			await tcs.Task;
		}
		Logger.Debug($"Sync.WaitAsync: ({source}) done");
	}

	public static bool Signal(SyncSrc source) {
		var q = GetQueue(source);
		Logger.Debug($"Sync.Signal: trying to signal to {source}");
		if (q.TryDequeue(out var tcs)) {
			tcs.TrySetResult(true);
			return true;
		}

		return false;
	}
}

public abstract record SyncSrc;

// When terminal user presses Enter (for managed input)
public record SyncSrcOnReturn(int Id) : SyncSrc;

// When command is received on ReceiveCommand
public record SyncSrcOnReceiveCmd(int Id) : SyncSrc;

