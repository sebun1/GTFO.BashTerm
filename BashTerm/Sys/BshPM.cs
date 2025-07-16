namespace BashTerm.Sys;

public class BshPM {
	internal BshIO? io = null;
	private readonly IProc? fgProcess; // current foreground process
	private readonly List<IProc> bgProcesses = new(); // background processes
	private readonly List<IProc> susProcesses = new(); // suspended processes
	private readonly Dictionary<int, IService> services = new();

	internal int fgPID = -1; // foreground process ID

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

	internal void LinkIO(BshIO io) {
		this.io = io;
	}
}
