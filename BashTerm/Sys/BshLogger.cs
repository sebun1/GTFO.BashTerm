using System;
using System.Collections.Generic;
using System.IO;

namespace BashTerm.Sys;

#nullable disable

public enum LogLevel {
	DEBUG,
	INFO,
	WARNING,
	ERROR,
	FATAL
}

public record LogEntry(LogLevel Level, string Message, Int64 Timestamp) {
	public override string ToString() {
		return Level switch {
			LogLevel.DEBUG => $"{Styles.C_Purple}[DEBUG]{Styles.C_End} {Message}",
			LogLevel.INFO => $" {Styles.C_Info}[INFO]{Styles.C_End} {Message}",
			LogLevel.WARNING => $" {Styles.C_Warning}[WARN]{Styles.C_End} {Message}",
			LogLevel.ERROR => $"{Styles.C_Error}[ERROR]{Styles.C_End} {Message}",
			LogLevel.FATAL => $"{Styles.C_Error}[FATAL] {Message}{Styles.C_End}",
			_ => $"[UNKWN] {Message}",
		};
	}

	public string ToFileString() {
		return $"[{Timestamp}] {Level}: {Message}";
	}
};

public class BshLogger {
	public static readonly BshLogger Instance = new();

	private readonly List<LogEntry> _logs = new();
	private readonly object _sync = new();
	private const int MaxEntries = 4096;

	private BshLogger() {
	}

	private static string Format(object data) => data.ToString();

	private void AddInternal(LogEntry entry) {
		lock (_sync) {
			_logs.Add(entry);
			if (_logs.Count > MaxEntries) {
				int remove = _logs.Count - MaxEntries;
				_logs.RemoveRange(0, remove);
			}
		}
	}

	public IReadOnlyList<LogEntry> GetEntries() {
		lock (_sync) {
			return _logs.ToArray();
		}
	}

	public int Count {
		get {
			lock (_sync) return _logs.Count;
		}
	}

	public void Clear() {
		lock (_sync) {
			_logs.Clear();
		}
	}

	/// <summary>
	/// Append all currently-stored logs to a file. Does not clear the buffer.
	/// </summary>
	public void DumpToFile(string path) {
		try {
			string[] lines;
			lock (_sync) {
				lines = new string[_logs.Count];
				for (int i = 0; i < _logs.Count; i++) lines[i] = _logs[i].ToFileString();
			}

			Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
			File.AppendAllLines(path, lines);
		}
		catch (Exception e) {
			BepLogger.Error($"Failed to dump logs to file '{path}': {e.Message}");
		}
	}

	/// <summary>
	/// Dump logs to file and then clear the in-memory buffer.
	/// </summary>
	public void DumpAndClear(string path) {
		DumpToFile(path);
		Clear();
	}

	public static void Debug(object msg) {
		if (Config.DEBUG) // TODO: Different debug toggle
			Instance.AddInternal(new LogEntry(LogLevel.DEBUG, Format(msg), BshTime.Time));
		BepLogger.Debug(msg);
	}

	public static void Info(object msg) {
		Instance.AddInternal(new LogEntry(LogLevel.INFO, Format(msg), BshTime.Time));
		BepLogger.Info(msg);
	}

	public static void Warn(object msg) {
		Instance.AddInternal(new LogEntry(LogLevel.WARNING, Format(msg), BshTime.Time));
		BepLogger.Warn(msg);
	}

	public static void Error(object msg) {
		Instance.AddInternal(new LogEntry(LogLevel.ERROR, Format(msg), BshTime.Time));
		BepLogger.Error(msg);
	}

	public static void Fatal(object msg) {
		Instance.AddInternal(new LogEntry(LogLevel.FATAL, Format(msg), BshTime.Time));
		BepLogger.Fatal(msg);
	}

	public static bool Ready => BshTime.Ready && BepLogger.Ready;
}
