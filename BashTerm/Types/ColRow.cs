namespace Bsh.Types;

public struct ColRow {
	public int Col;
	public int Row;
	public int X => Col;
	public int Y => Row;

	public ColRow(int col, int row) {
		Col = col;
		Row = row;
	}

	public override string ToString() {
		return $"(C:{Col}, R:{Row})";
	}
}
