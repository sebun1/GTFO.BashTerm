namespace Bsh.Sys;

public class IdManager {
	public const int IDMaxLimitDefault = 32768;
	public readonly int IdMaxLimit;

	private int _nextId = 1;
	private HashSet<int> _activeIds;
	private Dictionary<int, string> _idReferredExec = new();
	private Dictionary<int, string> _idDescriptions = new();

	public IdManager() {
		_activeIds = new();
		IdMaxLimit = IDMaxLimitDefault;
	}

	public IdManager(int idMaxLimit) {
		_activeIds = new();
		IdMaxLimit = idMaxLimit;
	}

	public bool GetId(out int id) {
		if (_nextId > IdMaxLimit) _nextId = 1;

		while (_activeIds.Contains(_nextId)) {
			if (_nextId > IdMaxLimit) {
				id = -1;
				return false;
			}

			_nextId++;
		}

		id = _nextId;
		_activeIds.Add(id);
		_nextId++;
		return true;
	}

	public bool SetDetail(int id, string execName, string desc) {
		if (!_activeIds.Contains(id)) return false;

		_idReferredExec[id] = execName;
		_idDescriptions[id] = desc;

		return true;
	}

	public bool ReleaseId(int pid) {
		if (_idReferredExec.ContainsKey(pid)) {
			_idReferredExec.Remove(pid);
		}

		if (_idDescriptions.ContainsKey(pid)) {
			_idDescriptions.Remove(pid);
		}

		return _activeIds.Remove(pid);
	}
}
