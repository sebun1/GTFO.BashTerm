namespace Bsh.Sys.Process;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class BshProgramAttribute : Attribute {
	public string Name { get; }

	public BshProgramAttribute(string name) {
		Name = name;
	}
}
