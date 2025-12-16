using System.Collections;

namespace Bsh.Sys.Render.History;

public class Runs {
	public readonly LineHistory Owner;
	public readonly Dictionary<RunType, List<Range>> RunMap = new();


	public Runs(LineHistory owner) {
		Owner = owner;

		foreach (RunType type in Enum.GetValues<RunType>())
			RunMap[type] = new List<Range>();
	}

	public void Add(RunType runType, Range range) {
		if ((RunType.Color & runType) != 0 && range is not ColorRange)
			throw new ArgumentException("Expected ColorRange for color run type.");
		int placement = 0;
		foreach (Range existing in RunMap[runType]) {
			// BUG: Does not check if runs coalesce after extending.
			if (existing.Extend(range))
				return;
			if (existing.End < range.Start)
				placement++;
		}

		RunMap[runType].Insert(placement, range);
	}

	public void Append(Runs after) {
		foreach (RunType type in Enum.GetValues<RunType>()) {
			Join(RunMap[type], after.RunMap[type]);
		}
	}

	private void Join(List<Range> self, List<Range> after) {
		if (after.Count == 0)
			return;

		// BUG: Does not check if runs coalesce after joining.
		foreach (Range run in after) {
			run.Offset(Owner.WidthVersion);
			if (self.Count != 0 && self[^1].Extend(run))
				continue;
			self.Add(run);
		}
	}

	public static Runs operator +(Runs a, Runs b) {
		a.Append(b);
		return a;
	}
}
