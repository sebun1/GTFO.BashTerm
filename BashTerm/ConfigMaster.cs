using BepInEx.Configuration;
using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;

namespace BashTerm;

internal static class ConfigMaster {
    public static ConfigFile conf;

    public static ConfigEntry<bool> EnableDebug;
    public static ConfigEntry<bool> EnableReactorVerifyAlias;
    public static ConfigEntry<bool> EnableUplinkVerifyAlias;

    static ConfigMaster() {
        string cfgPath = Path.Combine(Paths.ConfigPath, $"{Plugin.NAME}.cfg");
        conf = new ConfigFile(cfgPath, true);

        string sect = "Aliases";

        EnableReactorVerifyAlias = conf.Bind(
            sect,
            "Enable REACTOR_VERIFY Alias",
            true,
            "Maps \"RV\" to \"REACTOR_VERIFY\"");

        EnableUplinkVerifyAlias = conf.Bind(
            sect,
            "Enable UPLINK_VERIFY Alias",
            true,
            "Maps \"UV\" to \"UPLINK_VERIFY\"");

        sect = "Dev";

        EnableDebug = conf.Bind(
            sect,
            "Enable Debugging",
            false,
            "Enables debug logging");
    }

    public static bool EnableRVAlias { 
        get { return EnableReactorVerifyAlias.Value; }
    }

    public static bool EnableUVAlias {
        get { return EnableUplinkVerifyAlias.Value; }
    }

    public static bool DEBUG {
        get { return EnableDebug.Value; }
    }

    public static void Init() {
    }
}
