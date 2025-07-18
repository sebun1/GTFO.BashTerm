using LevelGeneration;
using UnityEngine;

namespace BashTerm.Sys;

public class BshPM {
	internal BshIO? io = null;

	internal int fgPID = -1; // foreground process ID
	private readonly IProc? fgProcess = null; // current foreground process

	private readonly Dictionary<int, IProc> bgProcesses = new(); // background processes
	private readonly Dictionary<int, IProc> susProcesses = new(); // suspended processes
	private readonly Dictionary<int, IService> services = new();

	public readonly int TerminalID;
	public readonly LG_ComputerTerminal? Terminal;

	public BshPM(int terminalID, LG_ComputerTerminal? terminal) {
		if (terminal == null) {
			throw new BSHException($"fatal: BshPM cannot initialize with a null terminal (terminalID={terminalID})");
		}
		TerminalID = terminalID;
		Terminal = terminal;
	}

	public bool CreateProcess(string name) {
		// TODO: Not implemented
		return true;
	}

	public int Update() {
		if (Terminal.enabled) {
			//
		}
		return -1;
	}

	internal void LinkIO(BshIO io) {
		this.io = io;
	}
}
