namespace Bsh.Sys.Completion;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class BshCompletionAttribute : Attribute {
	public string Name { get; }

	public BshCompletionAttribute(string name) {
		Name = name;
	}
}
