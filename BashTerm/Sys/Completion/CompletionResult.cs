using System.Text;

namespace Bsh.Sys.Completion;

public class CompletionResult {
	private List<string> _candidates;
	private int _candidateIdx = 0;
	private int _columnWidth = 0;
	private int _maxCandidateLen = 0;
	private const int CandidateSpacing = 2;
	private const int MaxCandidateDisplayLength = 28;
	public int Count => _candidates.Count;

	public CompletionResult() {
		_candidates = new();
	}

	public CompletionResult(List<string> candidates) {
		_candidates = candidates;
		foreach (string candidate in candidates) {
			if (candidate.Length > _maxCandidateLen) {
				_maxCandidateLen = candidate.Length;
			}
		}

		_columnWidth = _maxCandidateLen + CandidateSpacing;
	}

	public void Add(string candidate) {
		_candidates.Add(candidate);
	}


	public string Get() {
		return _candidates[_candidateIdx];
	}

	public string GetDisplay(int cols) {
		StringBuilder sb = new();
		int cpr = cols / _columnWidth;
		for (int i = 0; i < _candidates.Count; i++) {
			sb.Append(FmtCandidate(_candidates[i], i == _candidateIdx));
			if (i % cpr == cpr - 1) {
				sb.Append('\n');
			}
		}

		return sb.ToString();
	}

	private string FmtCandidate(string candidate, bool selected) {
		if (candidate.Length > MaxCandidateDisplayLength) {
			candidate = candidate.Substring(0, MaxCandidateDisplayLength - 3) + "...";
		}

		int padding = _columnWidth - candidate.Length;
		if (selected)
			candidate = $"{Styles.M_Accent}{candidate}{Styles.M_End}";
		candidate += new string(' ', padding);
		return candidate;
	}

	public void Next() {
		_candidateIdx = (_candidateIdx + 1) % _candidates.Count;
	}

	public void Prev() {
		_candidateIdx = (_candidateIdx - 1 + _candidates.Count) % _candidates.Count;
	}
}
