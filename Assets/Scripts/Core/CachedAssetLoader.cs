using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Provides a simplified interface for loading objects from Asset Bundles that
/// automatically caches the retrieved bundles.
/// </summary>
public class CachedAssetLoader : MonoBehaviour {
	[Tooltip("Should the cached be cleared when the owning object is disabled?")]
	[SerializeField]
	private bool clearOnDisable = true;
	public bool ClearOnDisable { get => clearOnDisable; set => clearOnDisable = value; }

	[Tooltip("When the cache is cleared, should associated assets still in the scene be unloaded?")]
	[SerializeField]
	private bool forceUnloadByDefault = false;
	public bool ForceUnloadByDefault { get => forceUnloadByDefault; set => forceUnloadByDefault = value; }

	private Dictionary<string, AssetBundle> Cache { get; } = new Dictionary<string, AssetBundle>();

	private void OnDisable() {
		if (clearOnDisable) Clear();
	}

	private void OnDestroy() {
		Clear();
	}

	/// <summary>
	/// Synchronously load an asset from a bundle and cache the loaded bundle.
	/// </summary>
	/// <typeparam name="T">The type of the loadable Unity Object asset.</typeparam>
	/// <param name="assetPath">The path to the asset realtive to the StreamableAssetsPath.</param>
	/// <returns>The loaded asset, or null if not found or loading failed.</returns>
	public T Load<T>(string assetPath) where T : Object {
		if (string.IsNullOrEmpty(assetPath)) {
			Debug.LogWarning("Attempted to load a null or empty asset path");
			return null;
		}

		var (bundlename, assetname) = assetPath.SplitOnLast('/');

		AssetBundle bundle;
		if (Cache.ContainsKey(bundlename)) {
			bundle = Cache[bundlename];
		}
		else {
			bundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bundlename));
			if (bundle == null) {
				Debug.LogWarning("Failed to load AssetBundle " + bundlename);
				return null;
			}
			Cache[bundlename] = bundle;
		}

		return bundle.LoadAsset<T>(assetname);
	}

	/// <summary>
	/// Asynchronously load an asset from a bundle and cache the loaded bundle.
	/// </summary>
	/// <typeparam name="T">The type of the loadable Unity Object asset.</typeparam>
	/// <param name="assetPath">The path to the asset realtive to the StreamableAssetsPath.</param>
	/// <param name="callback">The callback to trigger when loading succeeds or fails. Value will be null on failure.</param>
	/// <returns>An enumerator for coroutine processing.</returns>
	public IEnumerator LoadAsync<T>(string assetPath, UnityAction<T> callback) where T : Object {
		if (string.IsNullOrEmpty(assetPath)) {
			Debug.LogWarning("Attempted to load a null or empty asset path");
			callback.Invoke(null);
			yield break;
		}

		var (bundleRelPath, assetname) = assetPath.SplitOnLast('/');

		AssetBundle bundle;
		if (Cache.ContainsKey(bundleRelPath)) {
			bundle = Cache[bundleRelPath];
		}
		else {
			var bundleAbsPath = Application.streamingAssetsPath + "/" + bundleRelPath;
			if (!File.Exists(bundleAbsPath)) {
				Debug.LogWarning("Attempted to load a bundle file that can't be found: " + bundleAbsPath);
				callback.Invoke(null);
				yield break;
			}
			var bundleRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, bundleRelPath));
			yield return bundleRequest;

			bundle = bundleRequest.assetBundle;
			if (bundle == null) {
				Debug.LogWarning("Failed to load AssetBundle " + bundleRelPath);
				callback.Invoke(null);
				yield break;
			}
			Cache[bundleRelPath] = bundle;
		}

		var assetRequest = bundle.LoadAssetAsync<T>(assetname);
		yield return assetRequest;

		if (assetRequest == null) {
			Debug.LogWarning("Failed to load Asset " + assetname + " from Bundle " + bundleRelPath);
			callback.Invoke(null);
			yield break;
		}
		callback(assetRequest.asset as T);
	}

	/// <summary>
	/// Unloads all cached asset bundles and clears the references.
	/// </summary>
	/// <param name="forceUnload">Should assets still in use be removed from the scene?</param>
	public void Clear(bool forceUnload) {
		foreach (var bundle in Cache.Values) {
			bundle.Unload(forceUnload);
		}
		Cache.Clear();
	}

	/// <summary>
	/// Unloads all cached asset bundles and clears the references using default settings.
	/// </summary>
	public void Clear() {
		Clear(forceUnloadByDefault);
	}
}
