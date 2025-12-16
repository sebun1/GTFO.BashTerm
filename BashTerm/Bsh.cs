using Bsh.Sys;

#nullable disable

namespace Bsh;

public class Bsh {
	private const int InputLineMaxCol = 50;
	internal static BshSystem System { get; private set; }

	internal static void RegisterSystem(BshSystem system) {
		System = system;
	}
}
