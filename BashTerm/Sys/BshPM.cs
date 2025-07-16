namespace BashTerm.Sys;

public class BshPM {
	private readonly Dictionary<int, IProc> processes = new();
	private readonly Dictionary<int, IService> services = new();

	public readonly int TerminalID;

	public BshPM(int terminalID) {
		TerminalID = terminalID;
	}

	public bool CreateProcess(string name) {
		// TODO: Not implemented
		return true;
	}

	public int Update() {
		// TODO: Not implemented
		return -1;
	}
}
