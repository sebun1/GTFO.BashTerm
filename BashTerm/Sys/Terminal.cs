using Bsh.Types;
using LevelGeneration;

namespace Bsh.Sys;

public class Terminal : IUpdatable {
	public readonly int TerminalID;
	public LG_ComputerTerminal GTerminal { get; }
	public ProcessManager PM { get; }

	// TODO: We are missing a lot of attributes (maybe in Screen?)

	public Terminal(LG_ComputerTerminal gTerm) {
		if (gTerm == null)
			throw new BshSystemException("Terminal must be associated with a LG_ComputerTerminal instance.");

		GTerminal = gTerm;
		TerminalID = gTerm.m_serialNumber;
		PM = new ProcessManager(this, gTerm);
	}

	public void Update() {
		PM.Update();
	}
}
