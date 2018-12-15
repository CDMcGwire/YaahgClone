using System.Collections.Generic;

public static class ListExtensions {
	public static int FirstNonNullIndex<T>(this List<T> list) {
		for (var i = 0; i < list.Count; i++) if (list[i] != null) return i;
		return -1;
	}
}
