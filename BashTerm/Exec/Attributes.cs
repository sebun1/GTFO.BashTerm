namespace BashTerm.Exec;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)] // Don't currently want multiple, kind of complicates things
public class BshProcAttribute : Attribute{
	public string Name { get; }

	public BshProcAttribute(string name) { Name = name; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class BshSvcAttribute: Attribute{
	public string Name { get; }

	public BshSvcAttribute(string name) { Name = name; }
}
