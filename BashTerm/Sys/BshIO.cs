using System.Text;
using Il2CppSystem.Xml;

namespace BashTerm.Sys;

public class BshIO {
	public bool IsRaw = false;
	private readonly Queue<string> stdinQueue = new();
	private readonly StringBuilder lineBuffer = new();
	private BshPM? pm = null;

	internal void Update(string inputString) {
		if (IsRaw) return;

		bool escape = false;
		foreach (char c in inputString) {
			if (escape) {
				escape = false;
				lineBuffer.Append(c);
				continue;
			}
			switch (c) {
				case '\\':
					escape = true;
					break;
				case '\n':
					stdinQueue.Enqueue(lineBuffer.ToString());
					lineBuffer.Clear();
					break;
			}
		}
	}

	public bool ReadLine(int pid, out string? line) {
		if (pm.fgPID != pid || stdinQueue.Count == 0) {
			line = null;
			return false;
		}

		line = stdinQueue.Dequeue();
		return true;
	}

	public bool ReadChar(int pid, out char? c) {
		if (pm.fgPID != pid || lineBuffer.Length == 0) {
			c = null;
			return false;
		}

		c = lineBuffer.ToString()[0];
		lineBuffer.Remove(0, 1);
		return true;
	}

	internal void LinkPM(BshPM pm) {
		this.pm = pm;
	}
}
