using System.Collections.Generic;

public static class ListExtensions {
	public static int FirstNonNullIndex<T>(this List<T> list) {
		for (var i = 0; i < list.Count; i++) if (list[i] != null) return i;
		return -1;
	}

	public static void Merge<T>(this List<T> list, List<T> other) {
		var existing = new HashSet<T>(list);
		foreach (var item in other) {
			if (!existing.Contains(item)) list.Add(item);
		}
	}
}
