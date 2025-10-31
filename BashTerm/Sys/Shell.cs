namespace Bsh.Sys;

/// <summary>
/// Represents and manages the shell process in each terminal instance.
/// </summary>
internal class Shell {
	private List<string> _scrollback;
	private List<string> _lineInfos;

	internal Shell() {
	}
}
