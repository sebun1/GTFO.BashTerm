namespace Bsh.Sys.Completion;

public interface ICompletion {
	// TODO: Use reflection/attribute on a method instead of an interface?
	public CompletionResult Complete(string[] args);
}
