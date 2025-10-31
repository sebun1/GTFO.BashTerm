namespace Bsh.Sys;

public class IdManager {
	public const int IDMaxLimitDefault = 32768;
	public readonly int IdMaxLimit;

	private int _nextPid = 1;
	private HashSet<int> _activePids;

	public IdManager() {
		_activePids = new();
		IdMaxLimit = IDMaxLimitDefault;
	}

	public IdManager(int idMaxLimit) {
		_activePids = new();
		IdMaxLimit = idMaxLimit;
	}

	public bool GetPid(out int pid) {
		if (_nextPid > IdMaxLimit) _nextPid = 1;

		while (_activePids.Contains(_nextPid)) {
			if (_nextPid > IdMaxLimit) {
				pid = -1;
				return false;
			}

			_nextPid++;
		}

		pid = _nextPid;
		_activePids.Add(pid);
		_nextPid++;
		return true;
	}

	public bool ReleasePid(int pid) {
		return _activePids.Remove(pid);
	}
}
