using Bsh.Sys.Render.History;

namespace Bsh.Sys.Render.Renderer;

public static class HistoryRenderer {
	private enum Token {
		White,
		Text,
		Tab
	}

	private struct RenderState {
		public RunsInterpreter Inter;
		public string Buffer;
		public string BufferBg;
		public string Word;
		public string WordBg;
		public int Col;
		public int WordSize;
		public bool BreakWord;

		public RenderState(Runs runs) {
			Inter = new RunsInterpreter(runs);
			Buffer = "";
			BufferBg = "";
			Word = "";
			WordBg = "";
			Col = 0;
			WordSize = 0;
			BreakWord = false;
		}
	}

	private static Token Classify(char c) {
		if (c == '\t') return Token.Tab;
		if (char.IsWhiteSpace(c)) return Token.White;
		return Token.Text;
	}

	private static void FlushWord(ref RenderState st) {
		if (st.WordSize == 0) {
			if (st.Word.Length > 0) {
				st.Buffer += st.Word;
				st.BufferBg += st.WordBg;
				st.Word = "";
				st.WordBg = "";
			}

			return;
		}

		st.Buffer += st.Word;
		st.BufferBg += st.WordBg;
		st.Word = "";
		st.WordBg = "";
		st.WordSize = 0;
		st.BreakWord = false;
	}

	private static void FlushLine(ref RenderState st, List<string> target, List<string> targetBg) {
		st.Buffer += st.Inter.FgCloseAll();
		st.BufferBg += st.Inter.BgCloseAll();
		target.Add(st.Buffer);
		targetBg.Add(st.BufferBg);
		st.Buffer = st.Inter.FgOpenAll();
		st.BufferBg = st.Inter.BgOpenAll();
		st.Col = 0;
	}

	private static void AddChar(ref RenderState st, char c) {
		st.Word += c;
		st.WordBg += st.Inter.BgChar();
		st.WordSize++;
		st.Col++;
	}

	private static void AppendOpen(ref RenderState st) {
		st.Word += st.Inter.FgOpen();
		st.WordBg += st.Inter.BgOpen();
	}

	private static void AppendClose(ref RenderState st) {
		st.Word += st.Inter.FgClose();
		st.WordBg += st.Inter.BgClose();
	}

	public static void RenderLogicalLine(LineHistory hist, List<string> target, List<string> targetBg, string line,
		Runs runs, uint noBreakLimit = 16) {
		var st = new RenderState(runs);
		for (int i = 0; i < line.Length; i++) {
			char ch = line[i];
			Token tk = Classify(ch);

			if (st.Col == hist.WidthVersion) {
				FlushWord(ref st);
				FlushLine(ref st, target, targetBg);
			} else if (tk != Token.Text) {
				FlushWord(ref st);
			}


			switch (tk) {
				case Token.Tab: {
					AppendOpen(ref st);
					int spaces = 4 - (st.Col % 4);
					for (int s = 0; s < spaces; s++) {
						AddChar(ref st, ' ');
					}

					AppendClose(ref st);
					FlushWord(ref st);
					st.Inter.Next();
					break;
				}
				case Token.Text: {
					if (st.WordSize == 0) {
						int len = GetWordLen(i, line);
						if (len > noBreakLimit) {
							st.BreakWord = true;
						} else if (st.Col + len > hist.WidthVersion) {
							FlushLine(ref st, target, targetBg);
						}
					}

					AppendOpen(ref st);
					AddChar(ref st, ch);
					AppendClose(ref st);
					st.Inter.Next();
					break;
				}
				case Token.White: {
					AppendOpen(ref st);
					AddChar(ref st, ch);
					AppendClose(ref st);
					FlushWord(ref st);
					st.Inter.Next();
					break;
				}
			}
		}

		FlushWord(ref st);
		FlushLine(ref st, target, targetBg);
	}

	private static int GetWordLen(int col, string line) {
		for (int c = col; c < line.Length; c++) {
			if (Classify(line[c]) != Token.Text) return c - col;
		}

		return line.Length - col;
	}
}
