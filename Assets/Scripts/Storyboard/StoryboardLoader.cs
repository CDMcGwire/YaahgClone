using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles loading storyboards frome a given relative path.
/// </summary>
[RequireComponent(typeof(CachedAssetLoader))]
public class StoryboardLoader : MonoBehaviour {
	private StoryboardLoader instance;
	public StoryboardLoader Instance {
		get {
			if (instance == null) instance = FindObjectOfType<StoryboardLoader>();
			return instance;
		}
	}

	/// <summary>Reference to the cached asset loader to use for grabbing the storyboards.</summary>
	[SerializeField]
	[HideInInspector]
	private CachedAssetLoader assetLoader;

	private Dictionary<string, StoryboardData> CachedBoardData { get; } = new Dictionary<string, StoryboardData>();

	private void Start() {
		assetLoader = GetComponent<CachedAssetLoader>();
	}

	/// <summary>
	/// Load a Storyboard from a data URI "data:*".
	/// </summary>
	/// <param name="path">
	/// The path to the required storyboard of pattern {asset-bundle}/{text-asset}/{storyboard-name} 
	/// if not cached, or simply the storyboard name if part of an already cached board.
	/// </param>
	/// <param name="onLoaded">Callback to Invoke when loaded.</param>
	public void LoadData(string path, UnityAction<StoryboardData> onLoaded) {
		// Data based storyboards are cached by the storyboard name. The initial storyboard is named after the asset.
		var (assetPath, boardname) = path.SplitOnLast('/');
		// Storyboard data already loaded. Trigger callback immediately.
		if (CachedBoardData.ContainsKey(boardname)) {
			onLoaded.Invoke(CachedBoardData[boardname]);
			return;
		}
		// Load storyboard data, cache, and give callback
		StartCoroutine(assetLoader.LoadAsync<TextAsset>(
			path,
			textAsset => {
				if (textAsset == null) return;
				var parser = new StoryboardParser();
				var boardData = parser.Parse(textAsset);
				foreach (var entry in boardData) CachedBoardData[entry.Key] = entry.Value;
				onLoaded.Invoke(CachedBoardData[boardname]);
			}
		));
	}

	/// <summary>
	/// Load a Storyboard from a prefab URI "prefab:*".
	/// </summary>
	/// <param name="path">The path to the required storyboard of pattern {asset-bundle}/{storyboard-prefab}</param>
	/// <param name="onLoaded">Callback to Invoke when loaded.</param>
	public void LoadPrefab(string path, UnityAction<Storyboard> onLoaded) {
		StartCoroutine(assetLoader.LoadAsync(path, onLoaded));
	}
}
