using UnityEngine;
using Color = System.Drawing.Color;

namespace Bsh.Sys.Render;

internal record GridCellInfo {
	public bool Bold;
	public bool Underline;
	public bool Strikethrough;
	public Color Color;
}

public record GridLineInfo {
	public bool IsContinuation; // If this continues a previous line
}

/// <summary>
/// Represents the rendered grid of characters of a terminal pane.
/// </summary>
public class Pane {
	private char[] _grid;
	private GridCellInfo[] _gridCells;
	private GridLineInfo[] _gridLines;
	private uint _width;
	private uint _height;
	private uint _cursorPos;

	private bool _hasShell;
	private Shell? _linkedShell;

	internal Pane(uint width, uint height) {
		_width = width;
		_height = height;
		_grid = new char[width * height];
		_gridCells = new GridCellInfo[width * height];
		_gridLines = new GridLineInfo[height];
		_hasShell = false;
		_linkedShell = null;
	}

	internal Pane(uint width, uint height, Shell shell) {
		_width = width;
		_height = height;
		_grid = new char[width * height];
		_gridCells = new GridCellInfo[width * height];
		_gridLines = new GridLineInfo[height];
		_hasShell = true;
		_linkedShell = shell;
	}

	internal void RegisterShell(Shell shell) {
		_hasShell = true;
		_linkedShell = shell;
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="newWidth"></param>
	/// <param name="newHeight"></param>
	/// <returns>true if there is overflow content</returns>
	public bool Resize(uint newWidth, uint newHeight) {
		// TODO: Manipulate overflow to shell scrollback
		throw new NotImplementedException();
	}

	/// <summary>
	/// Returns the lines currently in view
	/// </summary>
	/// <returns></returns>
	internal List<string> GetLinesForRender() {
		throw new NotImplementedException();
	}

	public Vector2Int GetCursorPosition() {
		var row = (int)(_cursorPos / _width);
		var col = (int)(_cursorPos % _width);
		return new Vector2Int(col, row);
	}

	public Vector2Int GetPaneSize() {
		return new Vector2Int((int)_width, (int)_height);
	}

	public uint Width => _width;
	public uint Height => _height;
}
