using UnityEngine;
using System.Diagnostics;

namespace Bsh.Sys;

public class BshTime {
	private static Int64 _startTick;

	public static void Init() {
		_startTick = Stopwatch.GetTimestamp();
	}

	public static Int64 Time => Stopwatch.GetTimestamp() - _startTick;

	public static bool Ready => _startTick != 0;
}
