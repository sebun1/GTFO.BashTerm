namespace Bsh.Sys;

public class IdManager {
	public const int IDMaxLimitDefault = 32768;
	public readonly int IdMaxLimit;

	private int _nextId = 1;
	private HashSet<int> _activeIds;

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

	public bool ReleaseId(int pid) {
		return _activeIds.Remove(pid);
	}
}
