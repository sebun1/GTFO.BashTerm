using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bsh.Sys.Input;

[Flags]
public enum KeyModifier : byte {
	None = 0,
	Ctrl = 1 << 0,
	Alt = 1 << 1,
	Shift = 1 << 2
}

public readonly struct KeyStroke {
	public readonly KeyModifier Mods;
	public readonly KeyCode Key;
	public readonly char Char;
	public readonly bool Repeat;

	public KeyStroke(KeyCode key, KeyModifier mods, bool repeat = false) {
		Key = key;
		Mods = mods;
		Char = '\0';
		Repeat = repeat;
	}

	public KeyStroke(KeyCode key, KeyModifier mods, char c, bool repeat = false) {
		Key = key;
		Mods = mods;
		Char = c;
		Repeat = repeat;
	}

	public bool IsChar() {
		return Char != '\0';
	}

	public override bool Equals([NotNullWhen(true)] object? obj) {
		if (obj is KeyStroke ks) {
			return this == ks;
		}

		return false;
	}

	public override int GetHashCode() {
		return HashCode.Combine(Key, Mods, Char);
	}

	public static bool operator ==(KeyStroke a, KeyStroke b) {
		return a.Key == b.Key && a.Mods == b.Mods && a.Char == b.Char;
	}

	public static bool operator !=(KeyStroke a, KeyStroke b) {
		return !(a == b);
	}

	public override string ToString() {
		char modCtrl = (Mods & KeyModifier.Ctrl) != 0 ? 'C' : '-';
		char modAlt = (Mods & KeyModifier.Alt) != 0 ? 'A' : '-';
		char modShift = (Mods & KeyModifier.Shift) != 0 ? 'S' : '-';
		string repeatStr = Repeat ? " (repeat)" : "";

		if (IsChar()) {
			return
				$"KeyStroke: Key={Key.ToString()}, Char='{Char}'({(int)Char}), Mods=[{modCtrl}{modAlt}{modShift}]{repeatStr}";
		} else {
			return $"KeyStroke: Key={Key.ToString()}, Mods=[{modCtrl}{modAlt}{modShift}]{repeatStr}";
		}
	}
}
