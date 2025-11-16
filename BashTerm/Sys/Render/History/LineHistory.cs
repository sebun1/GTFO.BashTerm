using Bsh.Sys.Render.Renderer;
using SNetwork;
using TMPro;

namespace Bsh.Sys.Render.History;

public class LineHistory {
	/// <summary>
	/// Logical lines stored in the history.
	/// </summary>
	private List<string> _lines = new();

	/// <summary>
	/// Run info for each logical line.
	/// </summary>
	private List<Runs> _lineRuns = new();

	/// <summary>
	/// The
	/// Recompute when WidthVersion changes.
	/// </summary>
	private List<string> _renderCache = new();

	/// <summary>
	///
	/// Recompute when WidthVersion changes.
	/// </summary>
	private List<string> _renderCacheBg = new();

	private bool _lineIncomplete = false;

	public uint WidthVersion { get; private set; } = 0;
	public const uint TabVersion = 8; // TODO: possibly make this adjustable

	public LineHistory(uint widthVersion) {
		WidthVersion = widthVersion;
	}

	public void Append(string line, Runs runs, string rendered, string renderedBg, uint lineWidth,
		bool terminate = true) {
		Recompute(lineWidth);
		if (_lineIncomplete) {
			_lines[^1] += line;
			_lineRuns[^1].Append(runs);
		} else {
			_lines.Add(line);
			_lineRuns.Add(runs);
		}

		_renderCache.Add(rendered);
		_renderCacheBg.Add(renderedBg);
		_lineIncomplete = !terminate;
	}

	public void Recompute(uint newWidth) {
		if (newWidth == 0)
			throw new ArgumentException("Width cannot be zero.");
		if (newWidth == WidthVersion)
			return;
		WidthVersion = newWidth;
		_renderCache.Clear();
		_renderCacheBg.Clear();

		for (int i = 0; i < _lines.Count; i++)
			HistoryRenderer.RenderLogicalLine(this, _renderCache, _renderCacheBg, _lines[i], _lineRuns[i]);
	}

	public int LineCount => _lines.Count;

	public int Count() => _renderCache.Count;

	public string this[int index] => _renderCache[index];
}
