using BashTerm.Exec;
using BashTerm.Sys;
using BashTerm.Utils;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace BashTerm;

[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BasePlugin {
	public const string NAME = "Bsh";
	public const string GUID = "io.takina.gtfo." + NAME;
	public const string VERSION = "0.10.1";
	public const string BSH_VERSION = "1.0b";

	public override void Load() {
		// Global Initializers
		BepLogger.Setup();
		BshTime.Init();

		BshLogger.Info($"{NAME} {BSH_VERSION} [{GUID} @ {VERSION}]");
		BepLogger.Info("Patching...");
		Harmony h = new Harmony(GUID);
		AddComponent<BshSystem>();
		BashTerm.Config.Init();
		h.PatchAll(typeof(Patches.MainPatch));
		h.PatchAll(typeof(Patches.ScreenPatch));
		BepLogger.Info("Finished Patching");
	}
}
