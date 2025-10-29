using System.Reflection;
using BashTerm.Exec;
using BashTerm.Utils;
using UnityEngine;

namespace BashTerm.Sys;

internal class BshSystem : MonoBehaviour {
	private static bool _userRawMode;

	private float updateTimer = 0f;
	private const float updatePeriod = 0.025f;

	internal static readonly Dictionary<string, ProgramEntry> ProgramEntries = new();
	internal static readonly Dictionary<string, Type> SvcTypes = new();

	internal static Dictionary<int, ProcessManager> PM = new();

	private const int IdMaxLimit = 32768;
	private static int nextPID = 1;
	private static int nextSID = 1;
	private static int nextScID = 1;
	internal static HashSet<int> ActivePIDs = new(); // Process/Service IDs
	internal static HashSet<int> ActiveSIDs = new(); // Stream IDs aka FD
	internal static HashSet<int> ActiveScIDs = new(); // Screen IDs aka TTY

	// TODO: Probably add structured listeners for major events e.g. enter/exit, on exit/enter level, etc.

	public static bool UserRawMode {
		get { return _userRawMode; }
	}

	public static void ToggleRawMode() {
		_userRawMode = !_userRawMode;
	}

	public void Start() {
		int invalidCount = RegisterTypes(out var procCount, out var serviceCount);
		if (invalidCount > 0) {
			BepLogger.Warn(
				$"BshSystem: {invalidCount} types were not registered due to missing attributes or not implementing the required interfaces.");
		}

		BepLogger.Info($"BshSystem: Registered {procCount} processes and {serviceCount} services.");
	}

	private static int RegisterTypes(out int procCount, out int serviceCount) {
		ProgramEntries.Clear();
		SvcTypes.Clear();

		int errCount = 0;

		List<Type> allTypes = new();
		var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

		foreach (var assembly in loadedAssemblies) {
			try {
				allTypes.AddRange(assembly.GetTypes());
			}
			catch (ReflectionTypeLoadException) {
				BepLogger.Warn($"BshSystem: Could not load types from assembly: {assembly.FullName}");
			}
		}

		List<(string, Type)> procTypes = new();
		Dictionary<string, ICompletion> comps = new();

		foreach (var type in allTypes) {
			if (typeof(Program).IsAssignableFrom(type) && !type.IsAbstract) {
				var attr = type.GetCustomAttribute<BshProgramAttribute>();
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
			if (ProgramEntries.ContainsKey(procName)) {
				Type existent = ProgramEntries[procName].Type;
				Bsh.LogError("Sys",
					$"Process name <u>{procName}</u> is already registered to <u>{existent.FullName}</u>. Skipping registration for <u>{t.FullName}</u>.");
				errCount++;
				continue;
			}

			MethodInfo? getManifestMethod = t.GetMethod(
				"GetManifest",
				BindingFlags.Static | BindingFlags.Public,
				null,
				new Type[] { },
				null
			);
			if (getManifestMethod == null || getManifestMethod.ReturnType != typeof(ProgramManifest)) {
				Bsh.LogError("Sys",
					$"Class <u>{t.FullName}</u> of name <u>{procName}</u> is trying to define a process but does not have a compliant <u>static ProcManifest GetManifest()</u> method.");
				errCount++;
				continue;
			}

			ProgramManifest manifest = (ProgramManifest)getManifestMethod.Invoke(null, null)!;
			ProgramEntry pe = new ProgramEntry(t, manifest, comps.GetValueOrDefault(procName));
			ProgramEntries[procName] = pe;
		}

		procCount = ProgramEntries.Count;
		serviceCount = SvcTypes.Count;
		return errCount;
	}

	internal static int RequestPID() {
		// TODO: We are not considering the case when all IDs are taken, which is very unlikely but possible
		if (nextPID > IdMaxLimit || ActivePIDs.Contains(nextPID)) {
			nextPID = 1;
			while (ActivePIDs.Contains(nextPID)) {
				nextPID++;
			}
		}

		ActivePIDs.Add(nextPID);
		return nextPID++;
	}

	internal static bool ReleasePID(int pid) {
		return ActivePIDs.Remove(pid);
	}

	internal static int RequestSID() {
		if (nextSID > IdMaxLimit || ActiveSIDs.Contains(nextSID)) {
			nextSID = 1;
			while (ActiveSIDs.Contains(nextSID)) {
				nextSID++;
			}
		}

		ActiveSIDs.Add(nextSID);
		return nextSID++;
	}

	internal static bool ReleaseSID(int sid) {
		return ActiveSIDs.Remove(sid);
	}

	internal static int RequestScID() {
		if (nextScID > IdMaxLimit || ActiveScIDs.Contains(nextScID)) {
			nextScID = 1;
			while (ActiveScIDs.Contains(nextScID)) {
				nextScID++;
			}
		}

		ActiveScIDs.Add(nextScID);
		return nextScID++;
	}

	internal static bool ReleaseScID(int scid) {
		return ActiveScIDs.Remove(scid);
	}

	public void Update() {
		updateTimer += Time.deltaTime;
		if (updateTimer > updatePeriod) {
			updateTimer = 0f;
			foreach (var kvp in PM) {
				kvp.Value.Update();
			}
		}
	}
}

internal class ProgramEntry {
	public readonly Type Type;
	public readonly ProgramManifest Manifest;
	public readonly ICompletion? Completion;

	public ProgramEntry(Type t, ProgramManifest m, ICompletion? comp) {
		Type = t;
		Manifest = m;
		Completion = comp;
	}
}
