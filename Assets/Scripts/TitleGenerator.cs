using UnityEngine;

/// <summary>
/// Represents a handle to the list of available titles for use in getting a random character title.
/// </summary>
[CreateAssetMenu(fileName = "TitleGenerator", menuName = "Data/Title Generator")]
public class TitleGenerator : ScriptableObject {
	[SerializeField]
	private TextAsset titleFile;

	private string[] titles;

	public string Next {
		get {
			if (titles.Length < 1) {
				titles = titleFile.text.Split('\n');
			}
			return titles[Random.Range(0, titles.Length)];
		}
	}
}
