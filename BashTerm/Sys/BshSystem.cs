using System.Diagnostics;
using System.Reflection;
using Bsh.Sys.Completion;
using Bsh.Sys.Input;
using Bsh.Sys.Process;
using HarmonyLib;
using UnityEngine;

namespace Bsh.Sys;

internal class BshSystem : MonoBehaviour {
	public static BshSystem Instance { get; private set; }

	public static bool UserRawMode { get; private set; }

	private float updateTimer = 0f;
	private const float updatePeriod = 0.025f;

	internal static readonly Dictionary<string, ProgramEntry> ProgramEntries = new();
	internal static readonly Dictionary<string, Type> SvcTypes = new();

	internal InputRouter Input = new();
	internal Dictionary<int, ProcessManager> PM = new();
	internal IdManager Pid = new(); // Process/Service IDs

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(this);
	}

	public void Start() {
		int invalidCount = RegisterTypes(out var procCount, out var serviceCount);
		if (invalidCount > 0) {
			BepLogger.Warn(
				$"BshSystem: {invalidCount} types were not registered due to missing attributes or not implementing the required interfaces.");
		}

		BepLogger.Info($"BshSystem: Registered {procCount} processes and {serviceCount} services.");
	}

	public static void ToggleRawMode() {
		UserRawMode = !UserRawMode;
	}

	/// <summary>
	/// Registers all attributes in the calling assembly.
	/// </summary>
	public static void RegisterAll() {
		Assembly? assembly = new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.Assembly;
		if (assembly == null) throw new Exception("Bsh: Registration failed: could not get calling assembly.");

		AccessTools.GetTypesFromAssembly(assembly).Do(delegate(Type t) {
			if (t.GetCustomAttribute<BshProgramAttribute>() != null) {
				MethodInfo[] methods = t.GetMethods();
				// TODO
			}
		});
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
				var attr = type.GetCustomAttribute<BshServiceAttributes>();
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
				BshLogger.Error(
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
				BshLogger.Error(
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

	public int GetNewPid() {
		if (!Pid.GetId(out int pid)) {
			throw new BshSystemException("BshSystem: Could not allocate new PID.");
		}

		return pid;
	}

	public bool ReleasePid(int pid) {
		return Pid.ReleaseId(pid);
	}

	void Update() {
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
