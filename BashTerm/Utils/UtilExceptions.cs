using Bsh.Sys;

namespace Bsh.Utils;

internal class UtilException : BshException {
	public UtilException(string cause) : base($"[Util] >> {cause}") {
	}
}

internal class BadProgressBarException : UtilException {
	public BadProgressBarException(string cause) : base($"[BadProgressBar] >> {cause}") {
	}
}
