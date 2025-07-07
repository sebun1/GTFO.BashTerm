using BepInEx.Logging;

#nullable disable
namespace BashTerm.Utils;

internal static class Logger {
	private static ManualLogSource m_LogSource;

	public static void SetupFromInit(ManualLogSource logSource) => Logger.m_LogSource = logSource;

	private static string Format(object data) => data.ToString();

	public static void Debug(object msg) {
		if (ConfigMaster.DEBUG)
			m_LogSource.LogInfo((object)Format(msg));
	}

	public static void Info(object msg) => Logger.m_LogSource.LogInfo((object)Logger.Format(msg));

	public static void Warn(object msg) =>
		Logger.m_LogSource.LogWarning((object)Logger.Format(msg));

	public static void Error(object msg) => Logger.m_LogSource.LogError((object)Logger.Format(msg));

	public static void Fatal(object msg) => Logger.m_LogSource.LogFatal((object)Logger.Format(msg));
}
