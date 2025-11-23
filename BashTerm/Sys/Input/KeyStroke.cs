using UnityEngine;

namespace Bsh.Sys.Input;

[Flags]
public enum KeyModifier : byte {
	None = 0,
	Ctrl = 1 << 0,
	Alt = 1 << 1,
	Shift = 1 << 2
}

public enum KeyKind : byte {
	Char,
	Special
}

public enum KeySpecial {
	None,
	Backspace,
	Enter,
	Tab,
	Escape,
	UpArrow,
	DownArrow,
	LeftArrow,
	RightArrow,
	Home,
	End,
	PageUp,
	PageDown,
	Delete,
	Insert
}

public static class KeyConv {
	public static KeyCode Sp2KeyCode(KeySpecial key) {
		return key switch {
			KeySpecial.Backspace => KeyCode.Backspace,
			KeySpecial.Enter => KeyCode.Return,
			KeySpecial.Tab => KeyCode.Tab,
			KeySpecial.Escape => KeyCode.Escape,
			KeySpecial.UpArrow => KeyCode.UpArrow,
			KeySpecial.DownArrow => KeyCode.DownArrow,
			KeySpecial.LeftArrow => KeyCode.LeftArrow,
			KeySpecial.RightArrow => KeyCode.RightArrow,
			KeySpecial.Home => KeyCode.Home,
			KeySpecial.End => KeyCode.End,
			KeySpecial.PageUp => KeyCode.PageUp,
			KeySpecial.PageDown => KeyCode.PageDown,
			KeySpecial.Delete => KeyCode.Delete,
			KeySpecial.Insert => KeyCode.Insert,
			_ => KeyCode.None
		};
	}
}

public readonly struct KeyStroke {
	public readonly KeyKind Kind;
	public readonly KeyModifier Mods;
	public readonly char Key;
	public readonly KeySpecial SpecialKey;

	public KeyStroke(char key, KeyModifier mods) {
		Kind = KeyKind.Char;
		Mods = mods;
		Key = char.ToLower(key);
		SpecialKey = KeySpecial.None;
	}

	private KeyStroke(KeySpecial specialKey, KeyModifier mods) {
		Kind = KeyKind.Special;
		SpecialKey = specialKey;
		Mods = mods;
		Key = '\0';
	}

	public static KeyStroke Special(KeySpecial key, KeyModifier mods) {
		return new KeyStroke(key, mods);
	}

	public bool Is(char c) {
		return Kind == KeyKind.Char && c == Key;
	}

	public bool Is(KeySpecial sp) {
		return Kind == KeyKind.Special && SpecialKey == sp;
	}
}
