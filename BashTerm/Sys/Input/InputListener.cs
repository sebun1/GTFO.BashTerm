using UnityEngine;

namespace Bsh.Sys.Input;

public class InputListener {
	public readonly int Pid;
	private readonly Queue<KeyStroke> _keystrokeQueue = new();

	private readonly bool _isChild;
	private readonly InputListener? _parent;

	private int _activeChildPid;
	private readonly HashSet<int> _childListeners = new();

	/// <summary>
	/// If the listener is currently active and is capturing input
	/// </summary>
	public bool HasInput {
		get {
			if (_isChild)
				return _parent!.HasInput && _parent._activeChildPid == Pid;
			return BshSystem.Instance.Input.IsActive(Pid);
		}
	}

	internal InputListener(int pid) {
		Pid = pid;
		_activeChildPid = -1;
		_isChild = false;
		_parent = null;
	}

	internal InputListener(int pid, InputListener parent) {
		_parent = parent;
		Pid = pid;
		_activeChildPid = -1;
		_isChild = true;
	}

	/// <summary>
	/// Creates a child input listener for a given process ID.
	/// If one already exists, it is returned.
	/// </summary>
	/// <param name="pid"></param>
	/// <returns></returns>
	public InputListener CreateChild(int pid) {
		InputListener childIl = BshSystem.Instance.Input.CreateChildListener(pid, this);
		_childListeners.Add(pid);
		return childIl;
	}

	public void RemoveChild(int pid) {
		BshSystem.Instance.Input.RemoveListener(pid);
		_childListeners.Remove(pid);
		if (_activeChildPid == pid) {
			_activeChildPid = -1;
		}
	}

	internal void RemoveAllChildren() {
		foreach (int pid in _childListeners.ToArray()) {
			RemoveChild(pid);
		}

		_activeChildPid = -1;
	}

	public void SetActiveChild(int pid) {
		if (_childListeners.Contains(pid)) {
			_activeChildPid = pid;
		} else {
			throw new InputException($"Tried to set active child to non-existent child listener({pid}).");
		}
	}

	internal void Queue(KeyStroke ks) {
		if (_activeChildPid != -1) {
			if (BshSystem.Instance.Input.TryGetListener(_activeChildPid, out InputListener? child)) {
				child.Queue(ks);
				return;
			}

			throw new InputException($"ActiveChildPid({_activeChildPid}) set but child listener not found.");
		}

		_keystrokeQueue.Enqueue(ks);
	}

	public bool Get(out KeyStroke ks) {
		if (_keystrokeQueue.TryDequeue(out ks)) {
			return true;
		}

		return false;
	}

	public bool GetKey(KeyCode key) {
		return HasInput && UnityEngine.Input.GetKey(key);
	}

	public bool GetKeyDown(KeyCode key) {
		return HasInput && UnityEngine.Input.GetKeyDown(key);
	}

	public bool GetKeyUp(KeyCode key) {
		return HasInput && UnityEngine.Input.GetKeyUp(key);
	}

	public int Count => _keystrokeQueue.Count;

	public bool Available() {
		return _keystrokeQueue.Count > 0;
	}
}
