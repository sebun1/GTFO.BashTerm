using BepInEx.Logging;

#nullable disable
namespace Bsh;

internal static class BepLogger {
	private static ManualLogSource _mLogSource;
	public static bool Ready => _mLogSource != null;

	public static void Setup() {
		_mLogSource = Logger.CreateLogSource("io.takina.gtfo.Bsh");
	}

	public static void SetupFromInit(ManualLogSource logSource) => _mLogSource = logSource;

	private static string Format(object data) => data.ToString();

	public static void Debug(object msg) {
		if (Config.DEBUG)
			_mLogSource.LogDebug(Format(msg));
	}

	public static void Info(object msg) => _mLogSource.LogInfo(Format(msg));

	public static void Warn(object msg) => _mLogSource.LogWarning(Format(msg));

	public static void Error(object msg) => _mLogSource.LogError(Format(msg));

	public static void Fatal(object msg) => _mLogSource.LogFatal(Format(msg));
}
