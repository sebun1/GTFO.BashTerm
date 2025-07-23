namespace BashTerm.Sys;

public class Env {
	public Dictionary<string, string> mappings = new();

	public Env() {
	}

	public bool Add(string key, string value) {
		mappings[key] = value;
	}
}
