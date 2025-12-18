using Bsh.Sys.Input;
using Bsh.Sys.Render.Parser;
using Bsh.Sys.Stream;
using Bsh.Sys.Sh;
using Bsh.Types;
using UnityEngine;

namespace Bsh.Sys.Render;

internal record GridCellInfo {
	public bool Bold = false;
	public bool Underline = false;
	public bool Strikethrough = false;
	public bool Italic = false;
	public Rgb8 Color = Rgb8.FgDefault;
	public Rgb8 BgColor = Rgb8.BgDefault;
}

public record GridLineInfo {
	public bool IsContinuation; // If this continues a previous line
	public string CachedRender = "";
}

/// <summary>
/// Represents the rendered grid of characters of a terminal pane.
/// </summary>
public class Pane : IProcess {
	public int Pid { get; }

	private InputListener _inputListener;

	private char[,] _grid;
	private GridCellInfo[,] _gridCells;
	private GridLineInfo[] _gridLines;
	private uint _width;
	private uint _height;
	private ColRow _cursorPos;

	private bool _hasShell;
	private Shell? _linkedShell;

	private readonly PipeStream<byte> _stream = new();
	private readonly TextStreamParser _parser;

	public readonly PipeStreamWriter<byte> Writer;

	internal Pane(uint width, uint height) {
		_width = width;
		_height = height;
		_grid = new char[width, height];
		_gridCells = new GridCellInfo[width, height];
		_gridLines = new GridLineInfo[height];
		_cursorPos = new(0, 0);

		_hasShell = false;
		_linkedShell = null;

		_parser = new TextStreamParser(_stream.CreateReader());
		Writer = _stream.CreateWriter();
		Pid = BshSystem.Instance.GetNewPid();
		_inputListener = BshSystem.Instance.Input.CreateListener(Pid);
		BshSystem.Instance.Input.SetActive(Pid);
	}

	internal Pane(uint width, uint height, Shell shell) {
		_width = width;
		_height = height;
		_grid = new char[width, height];
		_gridCells = new GridCellInfo[width, height];
		_gridLines = new GridLineInfo[height];
		_cursorPos = new(0, 0);

		_hasShell = true;
		_linkedShell = shell;

		_parser = new TextStreamParser(_stream.CreateReader());
		Writer = _stream.CreateWriter();
		Pid = BshSystem.Instance.GetNewPid();
		_inputListener = BshSystem.Instance.Input.CreateListener(Pid);
	}

	internal void RegisterShell(Shell shell) {
		_hasShell = true;
		_linkedShell = shell;
	}

	/// <summary>
	/// Resize the pane propagating reflow effects
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

	public ColRow GetCursorPosition() {
		return _cursorPos;
	}

	public Vector2Int GetPaneSize() {
		return new Vector2Int((int)_width, (int)_height);
	}

	public uint Width => _width;
	public uint Height => _height;
}
