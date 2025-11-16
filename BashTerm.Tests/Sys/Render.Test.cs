using Bsh.Sys.Render.History;
using Bsh.Sys.Render.Renderer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Range = Bsh.Sys.Render.History.Range;

namespace Bsh.Tests.Sys;

[TestClass]
public class RenderTest {
	[TestMethod]
	public void HistoryRender() {
		string line = "what test verylongbut17over words say newline";
		LineHistory lh = new(20);
		List<string> cache = new();
		List<string> cacheBg = new();
		Runs runs = new Runs(lh);
		HistoryRenderer.RenderLogicalLine(lh, cache, cacheBg, line, runs);
		Assert.AreEqual(3, cache.Count);
		Assert.AreEqual("what test verylongbu", cache[0]);
		Assert.AreEqual("t17over words say ", cache[1]);
		Assert.AreEqual("newline", cache[2]);
	}

	[TestMethod]
	public void HistoryRenderWithRuns() {
		string line = "what test verylongbut17over words say newline";
		LineHistory lh = new(20);
		List<string> cache = new();
		List<string> cacheBg = new();
		Runs runs = new Runs(lh);
		runs.Add(RunType.FgColor, new ColorRange(9, 30, 255, 0, 127));
		runs.Add(RunType.Bold, new Range(0, 4));
		runs.Add(RunType.Italic, new Range(38, 45));
		runs.Add(RunType.BgColor, new ColorRange(38, 45, 255, 127, 0));
		HistoryRenderer.RenderLogicalLine(lh, cache, cacheBg, line, runs);

		Assert.AreEqual(3, cache.Count);
		Assert.AreEqual("<b>what</b> test<#FF007F10> verylongbu</color>", cache[0]);
		Assert.AreEqual("<#FF007F10>t17over wo</color>rds say ", cache[1]);
		Assert.AreEqual("<i>newline</i>", cache[2]);
		// Background expectations
		Assert.AreEqual("                    ", cacheBg[0]);
		Assert.AreEqual("                  ", cacheBg[1]);
		Assert.AreEqual("<mark=#FF7F0040>███████</mark>", cacheBg[2]);
	}
}
