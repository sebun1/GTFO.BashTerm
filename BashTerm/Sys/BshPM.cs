using LevelGeneration;
using UnityEngine;

namespace BashTerm.Sys;

public class BshPM {
	internal BshIO? io = null;

	internal int fgPID = -1; // foreground process ID
	private readonly Proc? fgProcess = null; // current foreground process

	private readonly Dictionary<int, Proc> bgProcesses = new(); // background processes
	private readonly Dictionary<int, Proc> susProcesses = new(); // suspended processes
	private readonly Dictionary<int, IService> services = new();

	public readonly int TerminalID;
	public readonly LG_ComputerTerminal? Terminal;

	public BshPM(int terminalID, LG_ComputerTerminal? terminal) {
		if (terminal == null) {
			throw new BshException($"fatal: BshPM cannot initialize with a null terminal (terminalID={terminalID})");
		}
		TerminalID = terminalID;
		Terminal = terminal;
	}

	public bool Execute(string name) {

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
