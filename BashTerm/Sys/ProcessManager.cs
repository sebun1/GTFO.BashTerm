using Bsh.Exec;
using Bsh.Parsers;
using Bsh.Types;
using LevelGeneration;
using UnityEngine;

namespace Bsh.Sys;

public class ProcessManager : IUpdatable {
	internal int fgPID = -1; // foreground process ID

	private readonly Dictionary<int, Program> _programs = new(); // background processes
	private readonly Dictionary<int, IService> _services = new();

	public readonly int TerminalID;
	private readonly Terminal _terminal;
	private readonly IdManager _pid;
	private readonly IdManager _sid;
	private readonly LG_ComputerTerminal _gTerminal;

	internal ProcessManager(Terminal owner, LG_ComputerTerminal term) {
		TerminalID = term.m_serialNumber;
		_terminal = owner;
		_gTerminal = term;
		_pid = new IdManager();
		_sid = new IdManager();
	}

	public bool Execute(string cmdstr) {
		if (!_pid.GetPid(out int pid)) {
			BepLogger.Error($"ProcessManager[{TerminalID}]: Could not allocate PID for new process.");
			return false;
		}

		VarCommand cmd = MainParser.Parse(cmdstr);
		// TODO: Execute program
		return true;
	}

	public void Update() {
		foreach (var programKvp in _programs) {
			Program p = programKvp.Value;
			if (p.State == eProgramState.Active)
				p.Update();
		}
	}
}
