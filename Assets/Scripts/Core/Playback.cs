using System.Collections.Generic;
using UnityEngine;

public class Playback : MonoBehaviour {
	private static Playback instance;
	private static Playback Instance {
		get {
			if (instance == null) instance = FindObjectOfType<Playback>();
			return instance;
		}
	}

	/// <summary>Set of objects requesting a pause.</summary>
	private HashSet<string> pauseRequests = new HashSet<string>();
	private static HashSet<string> PauseRequests { get { return Instance.pauseRequests; } }

	/// <summary>The time scale value when initially paused.</summary>
	private float originalTimeScale = 1.0f;
	private static float OriginalTimeScale { get { return Instance.originalTimeScale; } set { Instance.originalTimeScale = value; } }

	/// <summary>
	/// Issues a request to pause gameplay via the TimeScale global property. As long as
	/// one request remains, the game will remain paused.
	/// </summary>
	/// <param name="key">The name of the request.</param>
	public static void RequestPause(string key) {
		if (PauseRequests.Count < 1) {
			OriginalTimeScale = Time.timeScale;
			Time.timeScale = 0.0f;
		}
		PauseRequests.Add(key);
	}

	/// <summary>
	/// Deregisters the key from the list of pause requests. If the list is then empty, 
	/// then all requests have ended and the game can resume.
	/// </summary>
	/// <param name="key">The name of the request.</param>
	public static void ReleasePause(string key) {
		PauseRequests.Remove(key);
		if (PauseRequests.Count < 1) Time.timeScale = OriginalTimeScale;
	}

	private void Start() {
		if (Instance != this) {
			Destroy(gameObject);
			return;
		}
	}

	private void OnDestroy() {
		if (pauseRequests.Count > 0) Time.timeScale = OriginalTimeScale;
	}
}
