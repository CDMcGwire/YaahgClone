using UnityEngine;

public static class NameGenerator {
	private static string[] names;

	public static string Next {
		get {
			if (names == null) { names = Resources.Load<TextAsset>("Data/Names").text.Split('\n'); }
			return names[Random.Range(0, names.Length)];
		}
	}
}
