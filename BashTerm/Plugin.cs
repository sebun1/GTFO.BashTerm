using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace BashTerm;

[BepInPlugin(BashTerm.Plugin.GUID,
		BashTerm.Plugin.NAME,
		BashTerm.Plugin.VERSION)]
public class Plugin : BasePlugin
{
	public const string NAME = "BashTerm";
	public const string GUID = "io.takina.gtfo." + NAME;
	public const string VERSION = "0.1.0";

	public override void Load()
	{
		Logger.SetupFromInit(Log);
		Log.LogInfo(NAME + " " + GUID + " " + VERSION);
		Log.LogInfo("Enable RV Alias = " + ConfigMaster.EnableRVAlias);
		Log.LogInfo("Enable UV Alias = " + ConfigMaster.EnableUVAlias);
		Log.LogInfo("BashTerm: Patching...");
		Harmony.CreateAndPatchAll(typeof(Patch), GUID);
		Log.LogInfo("BashTerm: Finished Patching");
	}
}
