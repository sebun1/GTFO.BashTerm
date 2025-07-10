using BashTerm.Exec;
using BashTerm.Utils;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace BashTerm;

[BepInPlugin(BashTerm.Plugin.GUID, BashTerm.Plugin.NAME, BashTerm.Plugin.VERSION)]
public class Plugin : BasePlugin {
	public const string NAME = "BashTerm";
	public const string GUID = "io.takina.gtfo." + NAME;
	public const string VERSION = "0.3.0";
	public const string BSH_VERSION = "3.0a";

	public override void Load() {
		Logger.SetupFromInit(Log);
		Logger.Info(NAME + " " + GUID + " " + VERSION);
		Logger.Info("Patching...");
		int handlerCount = Dispatch.Initialize();
		ConfigMgr.Init();
		Logger.Debug($"{handlerCount} handlers registered");
		Harmony.CreateAndPatchAll(typeof(Patch), GUID);
		Logger.Info("Finished Patching");
	}
}
