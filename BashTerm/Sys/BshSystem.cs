using System.Reflection;
using BashTerm.Exec;
using BashTerm.Utils;
using UnityEngine;

namespace BashTerm.Sys;

internal class BshSystem : MonoBehaviour {

	private static bool _userRawMode;

	private float updateTimer = 0f;
	private const float updatePeriod = 0.1f;

	internal static readonly Dictionary<string, Type> ProcTypes = new();
	internal static readonly Dictionary<string, ICompletion> ProcCompletions = new();
	internal static readonly Dictionary<string, ProcEntry> ProcEntries = new();
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
			Log.Warn($"BashTerm: {invalidCount} types were not registered due to missing attributes or not implementing the required interfaces.");
		}
		Log.Info($"BashTerm: Registered {handlerCount} handlers and {serviceCount} services.");
	}

	private static int RegisterTypes(out int handlerCount, out int serviceCount) {
		ProcEntries.Clear();
		SvcTypes.Clear();

		int invalids = 0;

		var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();

		Dictionary<string, Type> procTypes = new();
		Dictionary<string, ICompletion> comps = new();

		foreach (var type in types) {
			if (typeof(IProc).IsAssignableFrom(type) && !type.IsAbstract) {
				var attr = type.GetCustomAttribute<BshProcAttribute>();
				if (attr != null) {
					procTypes[attr.Name] = type;
					continue;
				}
				invalids++;
			} else if (typeof(IService).IsAssignableFrom(type) && !type.IsAbstract) {
				var attr = type.GetCustomAttribute<BshSvcAttribute>();
				if (attr != null) {
					SvcTypes[attr.Name] = type;
					continue;
				}
				invalids++;
			} else if (typeof(ICompletion).IsAssignableFrom(type) && !type.IsAbstract) {
				var attr = type.GetCustomAttribute<BshCompletionAttribute>();
				if (attr != null) {
					ICompletion? comp = (ICompletion?)Activator.CreateInstance(type);
					comps[attr.Name] = comp;
					continue;
				}
				invalids++;
			}
		}

		foreach (Type type in procTypes) {

		}

		handlerCount = ProcEntries.Count;
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

internal class ProcEntry {
	public readonly Type Type;
	public readonly ProcManifest Manifest;
	public readonly ICompletion? Completion;

	public ProcEntry(Type t, ProcManifest m, ICompletion? comp) {
		Type = t;
		Manifest = m;
		Completion = comp;
	}
}
