using System.Text;
using UnityEngine;

namespace BashTerm.Sys;

public class BshAutocomplete {

}

public class BshAutocompleteResult{
	private List<string> candidates;
	private int cIdx = 0;
	private int columnWidth = 0;
	private int maxCandidateLen = 0;
	private const int candidateSpacing = 2;
	private static readonly int MaxCandidateDisplayLength = 28;
	public int Count => candidates.Count;

	public BshAutocompleteResult(List<string> candidates) {
		this.candidates = candidates;
		foreach (string candidate in candidates) {
			if (candidate.Length > maxCandidateLen) {
				maxCandidateLen = candidate.Length;
			}
		}
		columnWidth = maxCandidateLen + candidateSpacing;
	}

	public string Get() {
		return candidates[cIdx];
	}

	public string GetDisplay() {
		StringBuilder sb = new();
		int cpr = Screen.Cols / columnWidth;
		for (int i = 0; i < candidates.Count; i++) {
			sb.Append(FmtCandidate(candidates[i], i == cIdx));
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
		int padding = columnWidth - candidate.Length;
		if (selected)
			candidate = $"{Styles.M_Accent}{candidate}{Styles.M_End}";
		candidate += new string(' ', padding);
		return candidate;
	}

	public void Next() {
		cIdx = (cIdx + 1) % candidates.Count;
	}

	public void Prev() {
		cIdx = (cIdx - 1 + candidates.Count) % candidates.Count;
	}

}
