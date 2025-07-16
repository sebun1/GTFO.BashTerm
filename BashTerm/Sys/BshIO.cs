using System.Text;
using Il2CppSystem.Xml;

namespace BashTerm.Sys;

public class BshIO {
	public bool IsRaw = false;
	private readonly Queue<string> stdinQueue = new();
	private readonly StringBuilder lineBuffer = new();
	private readonly BshPM pm = null;
	private int fgPID = -1;

	internal void Update(string inputString) {
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
		if (fgPID != pid || stdinQueue.Count == 0) {
			line = null;
			return false;
		}

		line = stdinQueue.Dequeue();
		return true;
	}
}
