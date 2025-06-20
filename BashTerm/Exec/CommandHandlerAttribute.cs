namespace BashTerm.Exec;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)] // Don't currently want multiple, kind of complicates things
public class CommandHandlerAttribute : Attribute{
	public string Name { get; }

	public CommandHandlerAttribute(string name) { Name = name; }
}
