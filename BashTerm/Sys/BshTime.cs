using UnityEngine;
using System.Diagnostics;

namespace Bsh.Sys;

public class BshTime {
	private static Stopwatch? _stopwatch;

	public static void Init() {
		_stopwatch = new Stopwatch();
		_stopwatch.Start();
	}

	public static Int64 Time => _stopwatch?.ElapsedMilliseconds ?? -1;

	public static bool GetAlternateBool(int intervalMs = 500) {
		if (!Ready) return false;
		return Time / intervalMs % 2 == 0;
	}

	public static bool Ready => _stopwatch?.IsRunning ?? false;
}
