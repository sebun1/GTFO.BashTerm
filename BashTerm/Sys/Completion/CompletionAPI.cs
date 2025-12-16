namespace Bsh.Sys.Completion;

public static class CompletionAPI {
	/// <summary>
	/// Attempt to complete a gtfo terminal item in the current level.
	/// </summary>
	/// <param name="text">partial parameter string</param>
	/// <returns></returns>
	public static CompletionResult CompleteTerminalItem(string text) {
		return new CompletionResult();
	}

	/// <summary>
	/// Attempt to complete a bsh filesystem path.
	/// </summary>
	/// <param name="text">partial parameter string</param>
	/// <returns>completion result</returns>
	public static CompletionResult CompletePath(string text) {
		return new CompletionResult();
	}
}
