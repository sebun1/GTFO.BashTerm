using UnityEngine;
using Color = System.Drawing.Color;

namespace BashTerm.Sys.Render;

internal record GridCellInfo {
	public bool Bold;
	public bool Underline;
	public bool Strikethrough;
	public Color Color;
}

public record GridLineInfo {
	public bool IsContinuation; // If this continues a previous line
}

public class Pane {
	private char[] _grid;
	private GridCellInfo[] _gridCells;
	private GridLineInfo[] _gridLines;
	private uint _width;
	private uint _height;
	private uint _cursorPos;

	public Pane(uint width, uint height) {
		_width = width;
		_height = height;
		_grid = new char[width * height];
		_gridCells = new GridCellInfo[width * height];
		_gridLines = new GridLineInfo[height];
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="newWidth"></param>
	/// <param name="newHeight"></param>
	/// <returns>true if there is overflow content</returns>
	// public bool Resize(uint newWidth, uint newHeight, out List<???>) {
	// }

	/// <summary>
	/// Returns the lines currently in view
	/// </summary>
	/// <returns></returns>
	public List<string> GetLinesForRender() {
		throw new NotImplementedException();
	}

	public Vector2Int GetCursorPosition() {
		var row = (int)(_cursorPos / _width);
		var col = (int)(_cursorPos % _width);
		return new Vector2Int(col, row);
	}
}
