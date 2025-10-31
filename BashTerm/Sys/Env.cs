namespace Bsh.Sys;

public class Env {
	public Dictionary<string, string> mappings = new();

	public void Add(string key, string value) {
		mappings[key] = value;
	}

	public string this[string key] {
		get { return mappings.GetValueOrDefault(key, ""); }
		set { mappings[key] = value; }
	}
}
