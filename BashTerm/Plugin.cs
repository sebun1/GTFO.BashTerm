using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace BashTerm;

[BepInPlugin(BashTerm.Plugin.GUID, BashTerm.Plugin.NAME, BashTerm.Plugin.VERSION)]
public class Plugin : BasePlugin {
	public const string NAME = "BashTerm";
	public const string GUID = "io.takina.gtfo." + NAME;
	public const string VERSION = "0.2.1";
	public const string BSH_VERSION = "2.1a";

	public override void Load() {
		Logger.SetupFromInit(Log);
		Logger.Info(NAME + " " + GUID + " " + VERSION);
		Logger.Info("Patching...");
		Harmony.CreateAndPatchAll(typeof(Patch), GUID);
		ConfigMaster.Init();
		Logger.Info("Finished Patching");
	}
}
