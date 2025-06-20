namespace BashTerm;

public static class Il2CppBSUtils<T> {
	public static List<T> GetListFromIl2CppList(Il2CppSystem.Collections.Generic.List<T> list) {
		List<T> result = new List<T>();
		foreach (var i in list) {
			result.Add((T)i);
		}
		return result;
	}
}
