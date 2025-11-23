using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bsh.Sys.Input;

public class InputRouter {
	/// <summary>
	/// List of all special keys
	/// </summary>
	public static readonly KeySpecial[] SpecialKeys = (KeySpecial[])Enum.GetValues(typeof(KeySpecial));

	/// <summary>
	/// Dictionary mapping pids to their respective input listeners.
	/// </summary>
	private readonly Dictionary<int, InputListener> _listeners = new();

	/// <summary>
	/// Set of active listener pids.
	/// </summary>
	private readonly HashSet<int> _activeListeners = new();

	/// <summary>
	/// The current key modifier state
	/// </summary>
	private KeyModifier _currentMods = KeyModifier.None;

	/// <summary>
	/// The current input string from Unity's input system
	/// </summary>
	private string _currentInput = "";

	/// <summary>
	/// Updates the input router, propagating keystrokes to active listeners
	/// </summary>
	public void Update() {
		UpdateCurrentMods();
		PropagateKeystrokes();
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
			throw new BshSystemException("Cannot create a child InputListener for pid " + pid + " already exists");
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

	/// <summary>
	/// Pushes current keystrokes to active listeners
	/// </summary>
	private void PropagateKeystrokes() {
		// regular characters
		_currentInput = UnityEngine.Input.inputString;
		foreach (char ch in _currentInput) {
			Propagate(new KeyStroke(ch, _currentMods));
		}

		// special keys
		foreach (KeySpecial key in SpecialKeys) {
			if (UnityEngine.Input.GetKeyDown(KeyConv.Sp2KeyCode(key))) {
				Propagate(KeyStroke.Special(key, _currentMods));
			}
		}
	}

	/// <summary>
	/// Updates the current key modifier state
	/// </summary>
	private void UpdateCurrentMods() {
		_currentMods = KeyModifier.None;
		if (UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift))
			_currentMods |= KeyModifier.Shift;
		if (UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl))
			_currentMods |= KeyModifier.Ctrl;
		if (UnityEngine.Input.GetKey(KeyCode.LeftAlt) || UnityEngine.Input.GetKey(KeyCode.RightAlt))
			_currentMods |= KeyModifier.Alt;
	}

	/// <summary>
	/// Propagates a keystroke to all active input listeners
	/// </summary>
	/// <param name="keystroke"></param>
	private void Propagate(KeyStroke keystroke) {
		foreach (int pid in _activeListeners) {
			if (_listeners[pid].HasInput) _listeners[pid].Queue(keystroke);
		}
	}
}
