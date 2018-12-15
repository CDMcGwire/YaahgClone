using UnityEngine;

public static class TitleGenerator {
	private static string[] titles;

	public static string Next {
		get {
			if (titles == null) {
				titles = Resources.Load<TextAsset>("Data/StartingTitles").text.Split('\n');
			}
			return titles[Random.Range(0, titles.Length)];
		}
	}
}
