using Bsh.Types;
using InControl;
using LevelGeneration;
using TMPro;

namespace Bsh.Sys.Render;

/// <summary>
/// Represents an actual terminal display in GTFO.
/// </summary>
public class Display : IUpdatable {
	public readonly int TerminalID;
	private readonly Terminal _terminal;
	private readonly LG_ComputerTerminal _gTerminal;
	private TextMeshPro _textMesh;
	private DisplayLayout? _layout;

	public Display(Terminal owner, LG_ComputerTerminal gTerminal) {
		TerminalID = gTerminal.m_serialNumber;
		_terminal = owner;
		_gTerminal = gTerminal;
		_textMesh = gTerminal.m_text;
		_layout = null;
	}

	public Display(Terminal owner, LG_ComputerTerminal gTerminal, DisplayLayout layout) {
		TerminalID = gTerminal.m_serialNumber;
		_terminal = owner;
		_gTerminal = gTerminal;
		_textMesh = gTerminal.m_text;
		_layout = layout;
	}

	public Display(Terminal owner, LG_ComputerTerminal gTerminal, Pane pane) {
		TerminalID = gTerminal.m_serialNumber;
		_terminal = owner;
		_gTerminal = gTerminal;
		_textMesh = gTerminal.m_text;
		_layout = new LayoutSingle(pane);
	}

	public void SetDisplayLayout(DisplayLayout layout) {
		_layout = layout;
	}

	public void Update() {
		if (_layout == null) {
			// TODO: Handle this case?
			return;
		}

		_textMesh.text = _layout.GetRender();
	}
}
