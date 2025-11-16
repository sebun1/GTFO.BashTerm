using Bsh.Utils;

namespace Bsh.Sys.Render.History;

public class ColorRange : Range {
	public byte R;
	public byte G;
	public byte B;

	public ColorRange(uint start, uint end, byte r, byte g, byte b) : base(start, end) {
		R = r;
		G = g;
		B = b;
	}

	public override ColorRange Offset(uint offset) {
		Start += offset;
		End += offset;
		return this;
	}

	public override bool Connects(Range other) {
		if (!base.Connects(other))
			return false;
		if (other is not ColorRange)
			return false;
		ColorRange o = (ColorRange)other;
		return R == o.R && G == o.G && B == o.B;
	}

	public string ColorStr() {
		return $"{R:X2}{G:X2}{B:X2}";
	}
}
