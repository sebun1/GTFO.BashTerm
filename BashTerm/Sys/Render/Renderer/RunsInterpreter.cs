using Bsh.Sys.Render.History;
using Bsh.Utils;
using Range = Bsh.Sys.Render.History.Range;

namespace Bsh.Sys.Render.Renderer;

public class RunsInterpreter {
	private readonly Dictionary<RunType, List<Range>> _runs;

	private string _fgColor = "";
	private string _bgColor = "";

	private int _col;

	private Dictionary<RunType, bool> _active = new();
	private Dictionary<RunType, int> _runIdx = new();
	private Dictionary<RunType, uint> _runEnds = new();

	private static readonly RunType[] Attrs = Enum.GetValues<RunType>();
	private static readonly RunType[] AttrsReverse = GetRunTypeReverse();

	public RunsInterpreter(Runs runs) {
		_runs = runs.RunMap;
		foreach (RunType runType in Enum.GetValues<RunType>()) {
			_active[runType] = false;
			_runIdx[runType] = 0;
			_runEnds[runType] = 0;
		}
	}

	public string FgOpen() {
		string res = "";
		FgOpen(ref res);
		return res;
	}

	public void FgOpen(ref string res) {
		foreach (RunType type in Attrs) {
			if (type == RunType.BgColor)
				continue;
			if (!_active[type] &&
			    _runIdx[type] < _runs[type].Count &&
			    _runs[type][_runIdx[type]].Start == _col) {
				Range r = _runs[type][_runIdx[type]];
				if ((type & RunType.Color) != 0 && r is not ColorRange) {
					throw new RenderException("Expected ColorRange for Color run.");
				}

				_active[type] = true;
				_runEnds[type] = r.End;

				if ((type & RunType.FgColor) != 0) {
					_fgColor = ((ColorRange)r).ColorStr() + TMPUtil.TEXT_FG_ALPHA.ToString("X2");
					res += TMPUtil.OpenTag(type, param: _fgColor);
				} else {
					res += TMPUtil.OpenTag(type);
				}
			}
		}
	}

	public string FgClose() {
		string res = "";
		FgClose(ref res);
		return res;
	}

	public void FgClose(ref string res) {
		foreach (RunType type in AttrsReverse) {
			if (type == RunType.BgColor)
				continue;
			if (_active[type] && _runEnds[type] == _col + 1) {
				res += TMPUtil.CloseTag(type);
				_active[type] = false;
				_runIdx[type]++;
			}
		}
	}

	public string BgOpen() {
		string res = "";
		BgOpen(ref res);
		return res;
	}

	public void BgOpen(ref string res) {
		RunType type = RunType.BgColor;
		if (!_active[type] &&
		    _runIdx[type] < _runs[type].Count &&
		    _runs[type][_runIdx[type]].Start == _col) {
			Range r = _runs[type][_runIdx[type]];
			if (r is not ColorRange) {
				throw new RenderException("Expected ColorRange for BgColor run.");
			}

			_active[type] = true;
			_runEnds[type] = r.End;
			_bgColor = ((ColorRange)r).ColorStr() + TMPUtil.TEXT_BG_ALPHA.ToString("X2");
			res += TMPUtil.OpenTag(type, param: _bgColor);
		}
	}

	public string BgClose() {
		string res = "";
		BgClose(ref res);
		return res;
	}

	public void BgClose(ref string res) {
		RunType type = RunType.BgColor;
		if (_active[type] && _runEnds[type] == _col + 1) {
			res += TMPUtil.CloseTag(type);
			_active[type] = false;
			_runIdx[type]++;
		}
	}

	public string FgChange() {
		string res = "";
		FgClose(ref res);
		FgOpen(ref res);
		return res;
	}

	public string BgChange() {
		string res = "";
		BgClose(ref res);
		BgOpen(ref res);
		return res;
	}

	public string FgCloseAll() {
		string res = "";
		// Close in reverse order (no state mutation)
		foreach (RunType type in AttrsReverse) {
			if (type == RunType.BgColor) continue;
			if (_active[type]) res += TMPUtil.CloseTag(type);
		}

		return res;
	}

	public string FgOpenAll() {
		string res = "";
		// Reopen in forward order (no state mutation)
		foreach (RunType type in Attrs) {
			if (type == RunType.BgColor) continue;
			if (_active[type]) {
				if ((type & RunType.FgColor) != 0)
					res += TMPUtil.OpenTag(type, _fgColor);
				else
					res += TMPUtil.OpenTag(type);
			}
		}

		return res;
	}

	public string BgCloseAll() {
		// Only background color currently
		return _active[RunType.BgColor] ? TMPUtil.CloseTag(RunType.BgColor) : "";
	}

	public string BgOpenAll() {
		return _active[RunType.BgColor] ? TMPUtil.OpenTag(RunType.BgColor, _bgColor) : "";
	}

	public char BgChar() {
		return _active[RunType.BgColor] ? '█' : ' ';
	}

	public bool End() {
		return true;
	}

	public void Next() {
		_col++;
	}

	private static RunType[] GetRunTypeReverse() {
		RunType[] types = new RunType[Attrs.Length];
		Attrs.CopyTo(types, 0);
		Array.Reverse(types);
		return types;
	}
}
