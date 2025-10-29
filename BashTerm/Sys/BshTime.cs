using UnityEngine;
using System.Diagnostics;

namespace BashTerm.Sys;

public class BshTime {
	private static Int64 _startTick;

	public static void Init() {
		_startTick = Stopwatch.GetTimestamp();
	}

	public static Int64 Time => Stopwatch.GetTimestamp() - _startTick;

	public static bool Ready => _startTick != 0;
}
