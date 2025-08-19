using BashTerm.Exec;
using BashTerm.Sys;
using BashTerm.Utils;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace BashTerm;

[BepInPlugin(BashTerm.Plugin.GUID, BashTerm.Plugin.NAME, BashTerm.Plugin.VERSION)]
public class Plugin : BasePlugin {
	public const string NAME = "Bsh";
	public const string GUID = "io.takina.gtfo." + NAME;
	public const string VERSION = "0.99.1";
	public const string BSH_VERSION = "99.1a";

	public override void Load() {
		Logr.SetupFromInit(Log);
		Logr.Info(NAME + " " + GUID + " " + VERSION);
		Logr.Info("Patching...");
		var h = new Harmony(GUID);
		//int handlerCount = Dispatch.Initialize();
		AddComponent<BshSystem>();
		ConfigMgr.Init();
		h.PatchAll(typeof(Patch));
		h.PatchAll(typeof(Patches.ScreenPatch));
		Logr.Info("Finished Patching");
	}
}
