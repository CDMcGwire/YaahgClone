using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor script to run AssetBundle management methods.
/// </summary>
public class BundleAssets {
	[MenuItem("Assets/Build All AssetBundles")]
	static void BuildAllAssetBundles() {
		if (!Directory.Exists(Application.streamingAssetsPath)) {
			Directory.CreateDirectory(Application.streamingAssetsPath);
		}
		BuildPipeline.BuildAssetBundles(
			Application.streamingAssetsPath, 
			BuildAssetBundleOptions.None, 
			EditorUserBuildSettings.activeBuildTarget
		);
	}
}
