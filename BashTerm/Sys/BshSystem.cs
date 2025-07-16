using System.Reflection;
using BashTerm.Exec;
using UnityEngine;
using Logger = BashTerm.Utils.Logger;

namespace BashTerm.Sys;

internal class BshSystem : MonoBehaviour {

	private static bool _userRawMode;

	private float updateTimer = 0f;
	private const float updatePeriod = 0.1f;

	internal static readonly Dictionary<string, Type> ProcTypes = new();
	internal static readonly Dictionary<string, ProcManifest> ProcManifests = new();

	internal static readonly Dictionary<string, Type> SvcTypes = new();

	internal static Dictionary<int, BshPM> PM = new();
	internal static Dictionary<int, BshIO> IO = new();

	public const int PidMaxLimit = 32768;
	private static int nextPid = 1;
	internal static HashSet<int> ActivePIDs = new();

	public static bool UserRawMode { get { return _userRawMode; } }
	public static void ToggleRawMode() { _userRawMode = !_userRawMode; }

	public void Start() {
		// Register Types for Processes and Services
		int invalidCount = RegisterTypes(out var handlerCount, out var serviceCount);
		if (invalidCount > 0) {
			Logger.Warn($"BashTerm: {invalidCount} types were not registered due to missing attributes or not implementing the required interfaces.");
		}
		Logger.Info($"BashTerm: Registered {handlerCount} handlers and {serviceCount} services.");
	}

	private static int RegisterTypes(out int handlerCount, out int serviceCount) {
		ProcTypes.Clear();
		SvcTypes.Clear();

		int invalids = 0;

		var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();

		foreach (var type in types) {
			if (typeof(IProc).IsAssignableFrom(type) && !type.IsAbstract) {
				var attr = type.GetCustomAttribute<BshProcAttribute>();
				if (attr != null) {
					ProcTypes[attr.Name] = type;
					continue;
				}
			} else if (typeof(IService).IsAssignableFrom(type) && !type.IsAbstract) {
				var attr = type.GetCustomAttribute<BshSvcAttribute>();
				if (attr != null) {
					SvcTypes[attr.Name] = type;
					continue;
				}
			}
			invalids++;
		}

		handlerCount = ProcTypes.Count;
		serviceCount = SvcTypes.Count;
		return invalids;
	}

	internal static int RequestPID() {
		if (nextPid > PidMaxLimit || ActivePIDs.Contains(nextPid)) {
			nextPid = 1;
			while (ActivePIDs.Contains(nextPid)) {
				nextPid++;
			}
		}

		ActivePIDs.Add(nextPid);
		return nextPid++;
	}

	internal static bool ReleasePID(int pid) {
		return ActivePIDs.Remove(pid);
	}

	public void Update() {
		updateTimer += Time.deltaTime;
		if (updateTimer > updatePeriod) {
			updateTimer = 0f;
			// Do stuff
		}
	}
}

public class BSHException : Exception {
	public BSHException(string message) : base(message) {}
}
