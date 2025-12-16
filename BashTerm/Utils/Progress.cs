using System.Text;
using Bsh.Sys.Render;
using Bsh.Sys;
using Bsh.Sys.Process;
using Bsh.Sys.Stream;
using UnityEngine;

namespace Bsh.Utils;

// TODO: This needs to be reworked to be coroutine-based.
public abstract class Progress {
	private const int NON_SCREEN_PROGRESS_LENGTH = 40;

	public enum eProgressType {
		Timed,
		Manual,
		Staged,
		Indeterminate
	}

	protected readonly TextStreamWriter Writer;
	protected readonly int DisplayWidth;

	protected readonly string Description;
	public readonly eProgressType Type;

	protected char ProgressChar = '=';
	protected bool ShowArrow = true;
	protected bool TwoLine = false;
	protected bool ShowTime = true;

	protected Progress(eProgressType type, string desc, ProgramContext ctx) {
		Type = type;
		Writer = new TextStreamWriter(ctx.StdOut);
		Description = desc;
		DisplayWidth = ctx.HasPane ? (int)ctx.Pane!.Width : NON_SCREEN_PROGRESS_LENGTH;
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
		int usedLength = effectiveStartLength + end.Length;
		return DisplayWidth - usedLength;
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

	[Flags]
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
	private readonly float _duration;
	private float _elapsed;
	private readonly int _count;

	public ProgressTimed(ProgramContext ctx, string desc, float duration, int count = -1) :
		base(eProgressType.Timed, desc, ctx) {
		_duration = duration;
		_elapsed = 0f;
		_count = count;
	}

	public void SetDelta(float deltaTime) {
		_elapsed += deltaTime;
	}

	public void UpdateDelta(float deltaTime) {
		if (IsDone()) return;
		SetDelta(deltaTime);
		Flush();
	}

	public override bool IsDone() {
		return _elapsed >= _duration;
	}

	public sealed override void Flush() {
		string start = Description;
		if (TwoLine) start += "\n ";
		start += $" {_elapsed / _duration:P1} ";
		string end = "";
		if (_count > 0) {
			end += $" ({_elapsed / _duration * _count:N0}/{_count:N0})";
		}

		end += $" {_duration:F1}s ";
		int progressLength = CalculateProgressLength(start, end);

		string finalLine = $"{start}{BuildBar(_elapsed / _duration, progressLength)}{end}{EndChar()}";
		Writer.TryWrite(finalLine);
	}
}

public class ProgressManual : Progress {
	// TODO: Not finished
	private float _progress;
	private readonly int _count;
	private int _currentCount = 0;

	public ProgressManual(ProgramContext ctx, string desc, int count = 1) :
		base(eProgressType.Manual, desc, ctx) {
		_progress = 0f;
		_count = count;
	}

	public void SetPercent(float value) {
		_progress = Math.Clamp(value, 0f, 1f);
		_currentCount = (int)(value * _count);
	}

	public void UpdatePercent(float value) {
		if (IsDone()) return;
		SetPercent(value);
		Flush();
	}

	public void SetCount(int c) {
		_currentCount = Mathf.Min(c, _count);
		_progress = 1f * _currentCount / _count;
	}

	public void UpdateCount(int c) {
		if (IsDone()) return;
		SetCount(c);
		Flush();
	}

	public override bool IsDone() {
		return _progress >= 1f;
	}

	public sealed override void Flush() {
		string start = Description;
		if (TwoLine) start += "\n ";
		start += $" {_progress:P1} ";
		string end = $" ({_currentCount}/{_count}) ";
		int progressLength = CalculateProgressLength(start, end);
		string finalLine = $"{start}{BuildBar(_progress, progressLength)}{end}{EndChar()}";
		Writer.TryWrite(finalLine);
	}
}

public class ProgressIndeterminate : Progress {
	private bool _finished;
	private int _barPosition = 0;
	private bool _isIncreasing = true;

	public ProgressIndeterminate(ProgramContext ctx, string desc) :
		base(eProgressType.Indeterminate, desc, ctx) {
		_finished = false;
	}

	public void SetDone(bool done) {
		_finished = done;
	}

	public void UpdateDone(bool done) {
		if (IsDone()) return;
		SetDone(done);
		Flush();
	}

	public override bool IsDone() {
		return _finished;
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
			bar = BuildBounceBar(_barPosition, progressLength);
		char endChar = IsDone() ? '\n' : '\r';
		string finalLine = $"{start}{bar}{end}{EndChar()}";
		_barPosition += _isIncreasing ? 1 : -1;
		if (_barPosition <= 0 || _barPosition >= progressLength - 3) {
			_isIncreasing = !_isIncreasing;
			_barPosition = Mathf.Clamp(_barPosition, 0, progressLength - 3);
		}

		Writer.TryWrite(finalLine);
	}
}

public class ProgressStaged : Progress {
	/// <summary>
	/// <code>stages</code> is a list of tuples where each tuple contains the stage name and the weight (or duration in seconds as well if not indeterminate) for that stage.
	/// </summary>
	private readonly List<(string, float)> _stages;

	private int _currentStage;
	private float _currentStageWeight;
	private float _currentWeight;
	private float _totalWeight;
	private bool _isIndeterminate;
	private int _barPosition;
	private bool _isIncreasing;

	public ProgressStaged(ProgramContext ctx, string desc, List<(string, float)> stages, bool indeterminate = false) :
		base(eProgressType.Staged, desc, ctx) {
		_stages = stages;
		_isIndeterminate = indeterminate;
		_currentStage = 0;
		_currentStageWeight = 0f;
		_totalWeight = stages.Sum(s => s.Item2);
		_barPosition = 0;
		_isIncreasing = true;
		if (_totalWeight <= 0f)
			throw new BadProgressBarException("total weight of ProgressStaged stages must be greater than 0");
	}

	public void SetStage(int stageIdx) {
		if (!_isIndeterminate) return;
		_currentStage = Mathf.Clamp(stageIdx, 0, _stages.Count - 1);
		_currentWeight = 0f;
		for (int i = 0; i < stageIdx; i++) {
			_currentWeight += _stages[i].Item2;
		}

		Flush();
	}

	public void SetNextStage() {
		SetStage(_currentStage + 1);
	}

	public void SetDelta(float deltaTime) {
		if (_isIndeterminate) return;
		_currentStageWeight += deltaTime;
		_currentWeight += deltaTime;
		if (_currentStageWeight > _stages[_currentStage].Item2) {
			_currentStage++;
			_currentStageWeight = 0f;
		}
	}

	public void UpdateDelta(float deltaTime) {
		if (IsDone()) return;
		SetDelta(deltaTime);
		Flush();
	}

	public override bool IsDone() {
		return _currentStage >= _stages.Count;
	}

	public override void Flush() {
		string overviewStart = Description;
		if (TwoLine) overviewStart += "\n ";
		overviewStart += $" {_currentWeight / _totalWeight:P1} ";
		string overviewEnd = $" STAGE {_currentStage + 1}/{_stages.Count} ";

		string stageStart = $" ({_currentStage + 1}) {_stages[_currentStage].Item1} ";
		string stageEnd;
		if (_isIndeterminate)
			stageEnd = $" ??s \n";
		else
			stageEnd = $" {_stages[_currentStage].Item2:F1}s \n";
		int overviewProgressLength = CalculateProgressLength(overviewStart, overviewEnd);
		int stageProgressLength = CalculateProgressLength(stageStart, stageEnd);

		string finalLine =
			$"{overviewStart}{BuildBar(_currentWeight / _totalWeight, overviewProgressLength)}{overviewEnd}";
		finalLine += stageStart;
		if (_isIndeterminate)
			finalLine += BuildBounceBar(_barPosition, stageProgressLength);
		else
			finalLine += BuildBar(_currentStageWeight / _stages[_currentStage].Item2, stageProgressLength);
		finalLine += stageEnd;
		finalLine += EndChar(offset: 1);
		_barPosition += _isIncreasing ? 1 : -1;
		if (_barPosition <= 0 || _barPosition >= stageProgressLength - 3) {
			_isIncreasing = !_isIncreasing;
			_barPosition = Mathf.Clamp(_barPosition, 0, stageProgressLength - 3);
		}

		Writer.TryWrite(finalLine);
	}
}
