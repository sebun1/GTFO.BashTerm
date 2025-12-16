using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bsh.Sys.Input;

public class InputRouter : MonoBehaviour {
	/// <summary>
	/// Dictionary mapping pids to their respective input listeners.
	/// </summary>
	private readonly Dictionary<int, InputListener> _listeners = new();

	/// <summary>
	/// Set of active listener pids.
	/// </summary>
	private readonly HashSet<int> _activeListeners = new();

	/// <summary>
	/// Records which keys are currently held down.
	/// </summary>
	private readonly HashSet<KeyCode> _keyDown = new();

	/// <summary>
	/// Updates the input router, propagating keystrokes to active listeners
	/// </summary>
	void OnGUI() {
		switch (Event.current.type) {
			case EventType.KeyDown:
				UpdateKeyDown();
				break;

			case EventType.KeyUp:
				UpdateKeyUp();
				break;
		}
	}

	/// <summary>
	/// Performs updates on key down events
	/// </summary>
	private void UpdateKeyDown() {
		bool isRepeat = _keyDown.Contains(Event.current.keyCode);
		bool isNullChar = Event.current.character == '\0';
		KeyModifier mods = KeyModifier.None;
		if (Event.current.control)
			mods |= KeyModifier.Ctrl;
		if (Event.current.shift)
			mods |= KeyModifier.Shift;
		if (Event.current.alt)
			mods |= KeyModifier.Alt;

		switch (Event.current.keyCode) {
			case KeyCode.None:
				switch (Event.current.character) {
					case '\0': // Null char
						return; // Ignore

					case '\b': // Backspace (possible)
					case '\r': // Carriage Return (possible)
					case '\t': // Tab
					case '\n': // Line Feed
						Propagate(new KeyStroke(Event.current.keyCode, mods, Event.current.character),
							isSpecialChar: true);
						break;

					default: // Regular character (likely unicode)
						Propagate(new KeyStroke(Event.current.keyCode, mods, Event.current.character));
						break;
				}

				break;

			case KeyCode.LeftControl:
			case KeyCode.RightControl:
			case KeyCode.LeftShift:
			case KeyCode.RightShift:
			case KeyCode.LeftAlt:
			case KeyCode.RightAlt:
			case KeyCode.LeftCommand:
			case KeyCode.RightCommand:
			case KeyCode.LeftWindows:
			case KeyCode.RightWindows:
				Propagate(new KeyStroke(Event.current.keyCode, mods, Event.current.character, isRepeat), isMod: true);
				break;

			default:
				Propagate(new KeyStroke(Event.current.keyCode, mods, Event.current.character, isRepeat));
				break;
		}

		_keyDown.Add(Event.current.keyCode);
	}

	/// <summary>
	/// Performs updates on key up events
	/// </summary>
	private void UpdateKeyUp() {
		_keyDown.Remove(Event.current.keyCode);
	}

	/// <summary>
	/// Propagates the given keystroke to all active listeners
	/// </summary>
	/// <param name="keystroke">constructed keystroke</param>
	/// <param name="isMod">whether this is a modifier keystroke</param>
	/// <param name="isSpecialChar">whether this is a special character (e.g. '\n' '\r')</param>
	private void Propagate(KeyStroke keystroke, bool isMod = false, bool isSpecialChar = false) {
		if (isMod) return; // TODO: Option to handle modifier-only keystrokes

		foreach (int pid in _activeListeners) {
			if (_listeners[pid].HasInput) _listeners[pid].Queue(keystroke);
		}
	}

	/// <summary>
	/// Creates an input listener for the given pid if one does not already exist
	/// </summary>
	/// <param name="pid">target pid</param>
	/// <returns>the created InputListener</returns>
	internal InputListener CreateListener(int pid) {
		if (!_listeners.ContainsKey(pid)) {
			_listeners[pid] = new InputListener(pid);
		}

		return _listeners[pid];
	}

	/// <summary>
	/// Creates a listener that is a child of the given parent listener
	/// </summary>
	/// <param name="pid">target pid</param>
	/// <param name="parent">the parent of the listener</param>
	/// <returns>the created InputListener</returns>
	/// <exception cref="BshSystemException">when creating for a pid that already has a listener</exception>
	internal InputListener CreateChildListener(int pid, InputListener parent) {
		if (!_listeners.ContainsKey(pid)) {
			_listeners[pid] = new InputListener(pid, parent);
		} else {
			throw new BshSystemException("Cannot create a child InputListener for pid " + pid +
			                             " as it already exists");
		}

		return _listeners[pid];
	}

	/// <summary>
	/// Tries to get the listener for the given pid
	/// </summary>
	/// <param name="pid">target pid</param>
	/// <param name="listener">InputListener object</param>
	/// <returns>true if listener exists</returns>
	internal bool TryGetListener(int pid, [NotNullWhen(true)] out InputListener? listener) {
		if (!_listeners.ContainsKey(pid)) {
			listener = null;
			return false;
		}

		listener = _listeners[pid];
		return true;
	}

	/// <summary>
	/// Removes the listener for the given pid
	/// </summary>
	/// <param name="pid">target pid</param>
	/// <returns>true if the listener exist and is removed</returns>
	internal bool RemoveListener(int pid) {
		if (!_listeners.ContainsKey(pid)) {
			return false;
		}

		_listeners[pid].RemoveAllChildren();
		_activeListeners.Remove(pid);
		_listeners.Remove(pid);
		return true;
	}

	/// <summary>
	/// Query if the listener with the given pid is active.
	/// Only looks for root listeners.
	/// </summary>
	/// <param name="pid">target pid</param>
	/// <returns>true if the listener is active</returns>
	internal bool IsActive(int pid) {
		return _activeListeners.Contains(pid);
	}

	/// <summary>
	/// Query if a listener exists for the given pid
	/// </summary>
	/// <param name="pid">target pid</param>
	/// <returns>true if the root/child listener exists</returns>
	internal bool HasListener(int pid) {
		return _listeners.ContainsKey(pid);
	}

	/// <summary>
	/// Sets the listener with the given pid to active
	/// </summary>
	/// <param name="pid">pid</param>
	/// <returns>true if listener exists and is set to active</returns>
	internal bool SetActive(int pid) {
		if (!TryGetListener(pid, out _))
			return false;
		return _activeListeners.Add(pid);
	}

	/// <summary>
	/// Sets the listener with the given pid to inactive
	/// </summary>
	/// <param name="pid"></param>
	/// <returns>true if listener exists and is set to inactive</returns>
	internal bool SetInactive(int pid) {
		if (!_listeners.ContainsKey(pid)) {
			return false;
		}

		return _activeListeners.Remove(pid);
	}
}
