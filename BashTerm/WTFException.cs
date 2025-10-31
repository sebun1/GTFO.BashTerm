namespace Bsh;

public class WTFException : Exception {
	public WTFException(string message) {
		throw new Exception("WTF Exception (like literally wtf, this should never throw): " + message);
	}
}
