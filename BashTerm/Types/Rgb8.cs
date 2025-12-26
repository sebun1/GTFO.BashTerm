namespace Bsh.Types;

public struct Rgb8 {
	public byte R;
	public byte G;
	public byte B;

	public Rgb8(byte r, byte g, byte b) {
		R = r;
		G = g;
		B = b;
	}

	public override string ToString() => $"#{ToHex()}";

	public string ToHex() => $"{R:X2}{G:X2}{B:X2}";

	// Implicit conversion from System.Drawing.Color -> Rgb8
	public static implicit operator Rgb8(System.Drawing.Color c) => new(c.R, c.G, c.B);

	// Implicit conversion from Rgb8 -> System.Drawing.Color
	public static implicit operator System.Drawing.Color(Rgb8 rgb) =>
		System.Drawing.Color.FromArgb(rgb.R, rgb.G, rgb.B);

	// Implicit conversion from UnityEngine.Color32
	public static implicit operator Rgb8(UnityEngine.Color32 c) => new(c.r, c.g, c.b);

	// Implicit conversion from Rgb8 -> UnityEngine.Color
	public static implicit operator UnityEngine.Color32(Rgb8 rgb) =>
		new(rgb.R, rgb.G, rgb.B, 255);

	// Implicit conversion from hex string ("#RRGGBB" or "RRGGBB") -> Rgb8
	public static implicit operator Rgb8(string hex) {
		if (string.IsNullOrWhiteSpace(hex)) throw new ArgumentException("Hex string is null/empty", nameof(hex));
		hex = hex.Trim();
		if (hex.StartsWith('#')) hex = hex.Substring(1);
		if (hex.Length != 6) throw new FormatException("Hex string must be 6 characters (#RRGGBB)");
		byte r = Convert.ToByte(hex.Substring(0, 2), 16);
		byte g = Convert.ToByte(hex.Substring(2, 2), 16);
		byte b = Convert.ToByte(hex.Substring(4, 2), 16);
		return new Rgb8(r, g, b);
	}

	public static Rgb8 FgDefault = new(255, 255, 255);
	public static Rgb8 BgDefault = new(0, 0, 0);
}
