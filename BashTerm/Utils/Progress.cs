using System.Text;
using BashTerm.Sys;
using UnityEngine;

namespace BashTerm.Utils;

public abstract class Progress {
	public enum eProgressType {
		Timed,
		Manual,
		Staged,
		Indeterminate
	}

	protected readonly PipeStream Stream;
	protected readonly string Description;
	public readonly eProgressType Type;

	protected char ProgressChar = '=';
	protected bool ShowArrow = true;
	protected bool TwoLine = false;
	protected bool ShowTime = true;

	protected Progress(eProgressType type, PipeStream stream, string desc) {
		this.Type = type;
		this.Stream = stream;
		this.Description = desc;
	}

	public void SetStyle(ProgStyle style) {
		foreach (ProgStyle.eStyles s in style.styles) {
			switch (s) {
				case ProgStyle.eStyles.DoubleStrike:
					ProgressChar = '=';
					break;
				case ProgStyle.eStyles.SingleStrike:
					ProgressChar = '-';
					break;
				case ProgStyle.eStyles.ShowArrow:
					ShowArrow = true;
					break;
				case ProgStyle.eStyles.NoArrow:
					ShowArrow = false;
					break;
				case ProgStyle.eStyles.TwoLine:
					TwoLine = true;
					break;
				case ProgStyle.eStyles.OneLine:
					TwoLine = false;
					break;
				case ProgStyle.eStyles.ShowTime:
					ShowTime = true;
					break;
				case ProgStyle.eStyles.NoTime:
					ShowTime = false;
					break;
				case ProgStyle.eStyles.None:
					break;
			}
		}
	}

	protected string EndChar(int offset = 0) {
		if (TwoLine) offset++;
		if (IsDone()) return "\n";
		return $"\x1B[{1 + offset}F\r";
	}

	protected string BuildBar(float percentage, int size) {
		size = Mathf.Max(size, 6);
		int progressLength = size - 2;

		float progress = Math.Clamp(percentage, 0f, 1f);
		int fillCount = (int)(progressLength * progress);

		var barBuilder = new StringBuilder(progressLength);
		barBuilder.Append(ProgressChar, fillCount);
		barBuilder.Append(' ', progressLength - fillCount);

		if (ShowArrow && fillCount > 0) {
			barBuilder[fillCount - 1] = '>';
		}

		if (IsDone()) {
			return $"[{barBuilder.Remove(barBuilder.Length - 4, 4)}{Styles.C_Info}DONE{Styles.C_End}]";
		}

		return $"[{barBuilder}]";
	}

	protected string BuildBounceBar(int position, int size) {
		var bounceBarSize = Mathf.Min(size / 8 * 2 + 1, 5);
		var bounceBarHalfSize = bounceBarSize / 2;
		var barSpaceSize = size - 2;
		var barBuilder = new StringBuilder(barSpaceSize + bounceBarSize - 1);
		barBuilder.Length = barSpaceSize + bounceBarSize - 1;
		for (int i = 0; i < barBuilder.Capacity; i++) {
			if (i >= position && i < position + bounceBarSize) {
				barBuilder[i] = ProgressChar;
			} else {
				barBuilder[i] = ' ';
			}
		}

		return $"[{barBuilder.ToString().Substring(bounceBarHalfSize, barSpaceSize)}]";
	}

	protected int CalculateProgressLength(string start, string end) {
		int lastNewlineIndex = start.LastIndexOf('\n');
		int effectiveStartLength = lastNewlineIndex >= 0 ? start.Length - lastNewlineIndex - 1 : start.Length;
		return Stream.Cols - effectiveStartLength - end.Length;
	}

	public abstract void Flush();
	public abstract bool IsDone();
}

public class ProgStyle {
	public static ProgStyle DoubleStrike => new ProgStyle(eStyles.DoubleStrike);
	public static ProgStyle SingleStrike => new ProgStyle(eStyles.SingleStrike);
	public static ProgStyle ShowArrow => new ProgStyle(eStyles.ShowArrow);
	public static ProgStyle NoArrow => new ProgStyle(eStyles.NoArrow);
	public static ProgStyle TwoLine => new ProgStyle(eStyles.TwoLine);
	public static ProgStyle OneLine => new ProgStyle(eStyles.OneLine);
	public static ProgStyle None => new ProgStyle(eStyles.None);
	public static ProgStyle ShowTime => new ProgStyle(eStyles.ShowTime);
	public static ProgStyle NoTime => new ProgStyle(eStyles.NoTime);

	public enum eStyles {
		DoubleStrike, // Use '=' as progress character
		SingleStrike, // Use '-' as progress character
		ShowArrow, // Show arrow at the end of progress bar
		NoArrow, // No arrow at the end of progress bar
		TwoLine, // Description shows on separate line
		OneLine, // Description shows on same line
		ShowTime,
		NoTime,
		None
	}

	internal List<eStyles> styles = new();

	private ProgStyle(eStyles s) {
		styles.Add(s);
	}

	public static ProgStyle operator |(ProgStyle x, ProgStyle y) {
		x.styles.AddRange(y.styles);
		return x;
	}
}

public class ProgressTimed : Progress {
	private readonly float duration;
	private float elapsed;
	private readonly int count;

	public ProgressTimed(PipeStream stream, string desc, float duration, int count = -1) :
		base(eProgressType.Timed, stream, desc) {
		this.duration = duration;
		this.elapsed = 0f;
		this.count = count;
	}

	public void SetDelta(float deltaTime) {
		elapsed += deltaTime;
	}

	public void UpdateDelta(float deltaTime) {
		if (IsDone()) return;
		SetDelta(deltaTime);
		Flush();
	}

	public override bool IsDone() {
		return elapsed >= duration;
	}

	public sealed override void Flush() {
		string start = Description;
		if (TwoLine) start += "\n ";
		start += $" {elapsed / duration:P1} ";
		string end = "";
		if (count > 0) {
			end += $" ({elapsed / duration * count:N0}/{count:N0})";
		}

		end += $" {duration:F1}s ";
		int progressLength = CalculateProgressLength(start, end);

		string finalLine = $"{start}{BuildBar(elapsed / duration, progressLength)}{end}{EndChar()}";
		Stream.Print(finalLine);
	}
}

public class ProgressManual : Progress {
	// TODO: Not finished
	private float progress;
	private readonly int count;
	private int currentCount = 0;

	public ProgressManual(PipeStream stream, string desc, int count = 1) :
		base(eProgressType.Manual, stream, desc) {
		this.progress = 0f;
		this.count = count;
	}

	public void SetPercent(float value) {
		progress = Math.Clamp(value, 0f, 1f);
		currentCount = (int)(value * count);
	}

	public void UpdatePercent(float value) {
		if (IsDone()) return;
		SetPercent(value);
		Flush();
	}

	public void SetCount(int c) {
		currentCount = Mathf.Min(c, count);
		progress = 1f * currentCount / count;
	}

	public void UpdateCount(int c) {
		if (IsDone()) return;
		SetCount(c);
		Flush();
	}

	public override bool IsDone() {
		return progress >= 1f;
	}

	public sealed override void Flush() {
		string start = Description;
		if (TwoLine) start += "\n ";
		start += $" {progress:P1} ";
		string end = $" ({currentCount}/{count}) ";
		int progressLength = CalculateProgressLength(start, end);
		string finalLine = $"{start}{BuildBar(progress, progressLength)}{end}{EndChar()}";
		Stream.Print(finalLine);
	}
}

public class ProgressIndeterminate : Progress {
	private bool finished;
	private int barPosition = 0;
	private bool isIncreasing = true;

	public ProgressIndeterminate(PipeStream stream, string desc) :
		base(eProgressType.Indeterminate, stream, desc) {
		finished = false;
	}

	public void SetDone(bool done) {
		finished = done;
	}

	public void UpdateDone(bool done) {
		if (IsDone()) return;
		SetDone(done);
		Flush();
	}

	public override bool IsDone() {
		return finished;
	}

	public sealed override void Flush() {
		string start = Description;
		if (TwoLine) start += "\n ";
		start += " ~ ";
		string end = $" ETA: ?? ";
		int progressLength = CalculateProgressLength(start, end);

		string bar;
		if (IsDone())
			bar =
				$"[{new string(' ', (progressLength - 6) / 2)}{Styles.C_Info}DONE{Styles.C_End}{new string(' ', progressLength - 6 - (progressLength - 6) / 2)}]";
		else
			bar = BuildBounceBar(barPosition, progressLength);
		char endChar = IsDone() ? '\n' : '\r';
		string finalLine = $"{start}{bar}{end}{EndChar()}";
		barPosition += isIncreasing ? 1 : -1;
		if (barPosition <= 0 || barPosition >= progressLength - 3) {
			isIncreasing = !isIncreasing;
			barPosition = Mathf.Clamp(barPosition, 0, progressLength - 3);
		}

		Stream.Print(finalLine);
	}
}

public class ProgressStaged : Progress {
	/// <summary>
	/// <code>stages</code> is a list of tuples where each tuple contains the stage name and the weight (or duration in seconds as well if not indeterminate) for that stage.
	/// </summary>
	private readonly List<(string, float)> stages;

	private int currentStage;
	private float currentStageWeight;
	private float currentWeight;
	private float totalWeight;
	private bool isIndeterminate;
	private int barPosition;
	private bool isIncreasing;

	public ProgressStaged(PipeStream stream, string desc, List<(string, float)> stages, bool indeterminate = false) :
		base(eProgressType.Staged, stream, desc) {
		this.stages = stages;
		this.isIndeterminate = indeterminate;
		this.currentStage = 0;
		this.currentStageWeight = 0f;
		this.totalWeight = stages.Sum(s => s.Item2);
		this.barPosition = 0;
		this.isIncreasing = true;
		if (totalWeight <= 0f)
			throw new BadProgressBarException("total weight of ProgressStaged stages must be greater than 0");
	}

	public void SetStage(int stageIdx) {
		if (!isIndeterminate) return;
		currentStage = Mathf.Clamp(stageIdx, 0, stages.Count - 1);
		currentWeight = 0f;
		for (int i = 0; i < stageIdx; i++) {
			currentWeight += stages[i].Item2;
		}

		Flush();
	}

	public void SetNextStage() {
		SetStage(currentStage + 1);
	}

	public void SetDelta(float deltaTime) {
		if (isIndeterminate) return;
		currentStageWeight += deltaTime;
		currentWeight += deltaTime;
		if (currentStageWeight > stages[currentStage].Item2) {
			currentStage++;
			currentStageWeight = 0f;
		}
	}

	public void UpdateDelta(float deltaTime) {
		if (IsDone()) return;
		SetDelta(deltaTime);
		Flush();
	}

	public override bool IsDone() {
		return currentStage >= stages.Count;
	}

	public override void Flush() {
		string overviewStart = Description;
		if (TwoLine) overviewStart += "\n ";
		overviewStart += $" {currentWeight / totalWeight:P1} ";
		string overviewEnd = $" STAGE {currentStage + 1}/{stages.Count} ";

		string stageStart = $" ({currentStage + 1}) {stages[currentStage].Item1} ";
		string stageEnd;
		if (isIndeterminate)
			stageEnd = $" ??s \n";
		else
			stageEnd = $" {stages[currentStage].Item2:F1}s \n";
		int overviewProgressLength = CalculateProgressLength(overviewStart, overviewEnd);
		int stageProgressLength = CalculateProgressLength(stageStart, stageEnd);

		string finalLine =
			$"{overviewStart}{BuildBar(currentWeight / totalWeight, overviewProgressLength)}{overviewEnd}";
		finalLine += stageStart;
		if (isIndeterminate)
			finalLine += BuildBounceBar(barPosition, stageProgressLength);
		else
			finalLine += BuildBar(currentStageWeight / stages[currentStage].Item2, stageProgressLength);
		finalLine += stageEnd;
		finalLine += EndChar(offset: 1);
		barPosition += isIncreasing ? 1 : -1;
		if (barPosition <= 0 || barPosition >= stageProgressLength - 3) {
			isIncreasing = !isIncreasing;
			barPosition = Mathf.Clamp(barPosition, 0, stageProgressLength - 3);
		}

		Stream.Print(finalLine);
	}
}
