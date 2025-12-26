using Bsh.Sys.Process;
using Bsh.Sys.Render.History;
using Bsh.Utils;

namespace Bsh.Sys.Sh;

/// <summary>
/// Represents and manages the shell process in each terminal instance.
/// </summary>
internal class Shell : Program {
	private LineHistory _lh;

	internal Shell() {
		_lh = new LineHistory(Constants.TermWidth);
	}

	public List<byte> GetDisplay() {
		var list = new List<byte>();
		return list;
	}
}
