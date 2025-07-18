using BepInEx.Logging;

#nullable disable
namespace BashTerm.Utils;

internal static class Log {
	private static ManualLogSource m_LogSource;

	public static void SetupFromInit(ManualLogSource logSource) => Log.m_LogSource = logSource;

	private static string Format(object data) => data.ToString();

	public static void Debug(object msg) {
		if (ConfigMgr.DEBUG)
			m_LogSource.LogInfo((object)Format(msg));
	}

	public static void Info(object msg) => Log.m_LogSource.LogInfo((object)Log.Format(msg));

	public static void Warn(object msg) =>
		Log.m_LogSource.LogWarning((object)Log.Format(msg));

	public static void Error(object msg) => Log.m_LogSource.LogError((object)Log.Format(msg));

	public static void Fatal(object msg) => Log.m_LogSource.LogFatal((object)Log.Format(msg));
}
