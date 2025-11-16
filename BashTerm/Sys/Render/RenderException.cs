namespace Bsh.Sys.Render;

public class RenderException : BshSystemException {
	public RenderException(string message) : base($"[RenderErr] >> {message}") {
	}
}
