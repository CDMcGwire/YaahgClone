using UnityEngine;

/// <summary>
/// Responsible for loading StoryboardData files and caching the constructed results.
/// Also maintains a reference to a reusable DataDrivenStoryboard.
/// </summary>
public class BoardDataLoader : MonoBehaviour {
	private static BoardDataLoader instance;
	public static BoardDataLoader Instance {
		get {
			if (instance == null) instance = FindObjectOfType<BoardDataLoader>();
			return instance;
		}
	}
}
