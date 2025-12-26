using UnityEngine;

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

	public static implicit operator Vector2Int(ColRow pos) {
		return new Vector2Int(pos.Col, pos.Row);
	}

	public static implicit operator Vector2(ColRow pos) {
		return new Vector2(pos.Col, pos.Row);
	}

	public static implicit operator ColRow(Vector2Int vec) {
		return new ColRow(vec.x, vec.y);
	}
}
