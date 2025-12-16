namespace Bsh.Sys.Filesystem;

public class Fs {
	public static string Home { get; private set; } = "";

	public enum DataScope {
		User,
		Roaming,
		Local,
		LocalLow,
	}

	public static string GetUserDataPath() {
		if (string.IsNullOrEmpty(Home)) {
			Home = GetDataPath(DataScope.User);
		}

		return Home;
	}

	private static string GetDataPath(DataScope scope) {
		// TODO: Configurable home path

		string root;

		switch (scope) {
			case DataScope.User: {
				root = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				break;
			}

			case DataScope.Local: {
				root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				break;
			}

			case DataScope.Roaming: {
				root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				break;
			}

			case DataScope.LocalLow: {
				root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData",
					"LocalLow");
				break;
			}

			default:
				throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
		}

		return Path.Combine(root, "bsh");
	}
}
