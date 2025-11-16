namespace Bsh.Sys.Render.History;

public class Range {
	public uint Start;
	public uint End;

	public Range(uint start, uint end) {
		if (end <= start) {
			throw new ArgumentException("End must be greater than Start");
		}

		Start = start;
		End = end;
	}

	public virtual Range Offset(uint offset) {
		Start += offset;
		End += offset;
		return this;
	}

	public virtual bool Connects(Range other) {
		return !(End < other.Start || other.End < Start);
	}

	private bool ExtendsStart(Range other) {
		return other.Start < Start && Connects(other);
	}

	private bool ExtendsEnd(Range other) {
		return other.End > End && Connects(other);
	}

	public bool Extend(Range other) {
		bool startExt = ExtendsStart(other);
		bool endExt = ExtendsEnd(other);

		if (startExt)
			Start = other.Start;
		if (endExt)
			End = other.End;
		return startExt || endExt;
	}
}
