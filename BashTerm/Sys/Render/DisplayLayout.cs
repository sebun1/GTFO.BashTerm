namespace Bsh.Sys.Render;

public abstract class DisplayLayout {
	public abstract uint Width { get; }
	public abstract uint Height { get; }

	/// <summary>
	/// Resize the layout to new dimensions
	/// </summary>
	/// <param name="newWidth">width to resize to, 0 to keep the same</param>
	/// <param name="newHeight">height to resize to, 0 to keep the same</param>
	public abstract void Resize(uint newWidth, uint newHeight);

	/// <summary>
	/// Gets the lines to render
	/// </summary>
	/// <returns>List of strings of the lines in the display layout</returns>
	public abstract IEnumerable<string> GetLines();

	/// <summary>
	/// Get the full render as a single string
	/// </summary>
	/// <returns>display string for the entire layout</returns>
	public abstract string GetRender();
}

public class LayoutSingle : DisplayLayout {
	private readonly Pane _pane;

	public LayoutSingle(Pane pane) {
		_pane = pane;
	}

	public override uint Width => _pane.Width;
	public override uint Height => _pane.Height;

	public override void Resize(uint newWidth, uint newHeight) {
		throw new NotImplementedException();
	}

	public override IEnumerable<string> GetLines() {
		return _pane.GetLinesForRender();
	}

	public override string GetRender() {
		return string.Join('\n', _pane.GetLinesForRender());
	}
}

// TODO: Fully implement + vertical layout
public class LayoutHorizontal : DisplayLayout {
	public override uint Width {
		get {
			uint totalWidth = 0;
			foreach (var pane in _panes) {
				totalWidth += pane.Width;
				totalWidth++;
			}

			return totalWidth - 1;
		}
	}

	public override uint Height => _panes[0].Height;

	private DisplayLayout[] _panes;

	public LayoutHorizontal(params DisplayLayout[] panes) {
		_panes = panes;
		// Standardize heights
	}

	public override void Resize(uint newWidth, uint newHeight) {
		throw new NotImplementedException();
	}

	public override IEnumerable<string> GetLines() {
		IEnumerable<string> lines = _panes[0].GetLines();
		for (int i = 1; i < _panes.Length; i++) {
			var paneLines = _panes[i].GetLines().ToArray();
			var combinedLines =
				lines.Zip(paneLines, (left, right) => left + $"{Styles.C_Accent}|{Styles.C_End}" + right);
			lines = combinedLines;
		}

		return lines;
	}

	public override string GetRender() {
		throw new NotImplementedException();
	}
}
