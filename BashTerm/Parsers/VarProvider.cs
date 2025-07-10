using LevelGeneration;

namespace BashTerm.Parsers;

/// <summary>
/// The default variable provider when parsing
/// </summary>
public class VarProvider {
	private LG_ComputerTerminal _term;
	public VarProvider(LG_ComputerTerminal term) {
		_term = term;
	}

	public bool TryGetVarValue(string varName, out string? value) {
		// Strict match
		switch (varName) {
			case "SHELL":
			case "shell":
				value = "BSH";
				return true;
			case "ZONE":
			case "zone":
				value =  $"ZONE_{_term.SpawnNode.m_zone.ID}";
				return true;
			case "ZONE_ID":
			case "zone_id":
				value = _term.SpawnNode.m_zone.ID.ToString();
				return true;
			case "AREA":
			case "area":
				value = _term.SpawnNode.m_area.m_navInfo.Suffix;
				return true;
		}

		// if (varName.StartsWith("LOG_")) {
		// 	try {
		// 		int logNo = int.Parse(varName.Substring("LOG_".Length));
		// 		var logNames = _term.GetLocalLogs().Keys;
		// 	} catch (FormatException e) {}
		// }
		value = null;
		return false;
	}
}
