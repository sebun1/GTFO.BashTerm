namespace Bsh.Sys;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class BshServiceAttributes : Attribute {
	public string Name { get; }

	public BshServiceAttributes(string name) {
		Name = name;
	}
}
