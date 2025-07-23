using BepInEx.Logging;

#nullable disable
namespace BashTerm.Utils;

internal static class Logr {
	private static ManualLogSource m_LogSource;

	public static void SetupFromInit(ManualLogSource logSource) => Logr.m_LogSource = logSource;

	private static string Format(object data) => data.ToString();

	public static void Debug(object msg) {
		if (ConfigMgr.DEBUG)
			m_LogSource.LogInfo((object)Format(msg));
	}

	public static void Info(object msg) => Logr.m_LogSource.LogInfo((object)Logr.Format(msg));

	public static void Warn(object msg) =>
		Logr.m_LogSource.LogWarning((object)Logr.Format(msg));

	public static void Error(object msg) => Logr.m_LogSource.LogError((object)Logr.Format(msg));

	public static void Fatal(object msg) => Logr.m_LogSource.LogFatal((object)Logr.Format(msg));
}
