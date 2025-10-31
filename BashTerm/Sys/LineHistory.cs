namespace Bsh.Sys;

public struct LineInfo {
	public int ScreenWidth;
	public uint RenderedLineCount;
	public List<int> NewLineIdx = new();
	public List<string> RenderCache = new();

	public LineInfo() {
		ScreenWidth = 0;
		RenderedLineCount = 0;
	}
}

public class LineHistory {
	private List<string> _lines = new();
	private List<LineInfo> _lineInfos = new();

	public LineHistory() {
	}
}
