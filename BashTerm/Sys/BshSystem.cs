using System.Reflection;
using BashTerm.Exec;
using BashTerm.Utils;
using UnityEngine;

namespace BashTerm.Sys;

internal class BshSystem : MonoBehaviour {

	private static bool _userRawMode;

	private float updateTimer = 0f;
	private const float updatePeriod = 0.05f;

	// internal static readonly Dictionary<string, Type> ProcTypes = new();
	// internal static readonly Dictionary<string, ICompletion> ProcCompletions = new();
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
		int invalidCount = RegisterTypes(out var procCount, out var serviceCount);
		if (invalidCount > 0) {
			Logr.Warn($"BshSystem: {invalidCount} types were not registered due to missing attributes or not implementing the required interfaces.");
		}
		Logr.Info($"BshSystem: Registered {procCount} processes and {serviceCount} services.");
	}

	private static int RegisterTypes(out int procCount, out int serviceCount) {
		ProcEntries.Clear();
		SvcTypes.Clear();

		int errCount = 0;

		List<Type> allTypes = new();
		var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

		foreach (var assembly in loadedAssemblies) {
			try {
				allTypes.AddRange(assembly.GetTypes());
			}
			catch (ReflectionTypeLoadException) {
				Logr.Warn($"BshSystem: Could not load types from assembly: {assembly.FullName}");
			}
		}

		List<(string, Type)> procTypes = new();
		Dictionary<string, ICompletion> comps = new();

		foreach (var type in allTypes) {
			if (typeof(Proc).IsAssignableFrom(type) && !type.IsAbstract) {
				var attr = type.GetCustomAttribute<BshProcAttribute>();
				if (attr != null) {
					procTypes.Add((attr.Name, type));
				}
			} else if (typeof(IService).IsAssignableFrom(type) && !type.IsAbstract) {
				var attr = type.GetCustomAttribute<BshSvcAttribute>();
				if (attr != null) {
					SvcTypes[attr.Name] = type;
				}
			} else if (typeof(ICompletion).IsAssignableFrom(type) && !type.IsAbstract) {
				var attr = type.GetCustomAttribute<BshCompletionAttribute>();
				if (attr != null) {
					ICompletion? comp = (ICompletion?)Activator.CreateInstance(type);
					if (comp == null)
						continue;
					comps[attr.Name] = comp;
				}
			}
		}

		foreach ((string procName, Type t) in procTypes) {
			if (ProcEntries.ContainsKey(procName)) {
				Type existent = ProcEntries[procName].Type;
				Bsh.LogError("Sys", $"Process name <u>{procName}</u> is already registered to <u>{existent.FullName}</u>. Skipping registration for <u>{t.FullName}</u>.");
				errCount++;
				continue;
			}

			MethodInfo? getManifestMethod = t.GetMethod(
				"GetManifest",
				BindingFlags.Static | BindingFlags.Public,
				null,
				new Type[] {},
				null
			);
			if (getManifestMethod == null || getManifestMethod.ReturnType != typeof(ProcManifest)) {
				Bsh.LogError("Sys", $"Class <u>{t.FullName}</u> of name <u>{procName}</u> is trying to define a process but does not have a compliant <u>static ProcManifest GetManifest()</u> method.");
				errCount++;
				continue;
			}
			ProcManifest manifest = (ProcManifest)getManifestMethod.Invoke(null, null)!;
			ProcEntry pe = new ProcEntry(t, manifest, comps.GetValueOrDefault(procName));
			ProcEntries[procName] = pe;
		}

		procCount = ProcEntries.Count;
		serviceCount = SvcTypes.Count;
		return errCount;
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
