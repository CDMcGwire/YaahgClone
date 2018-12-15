using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {
	public UnityEvent OnLevelBeginLoad = new UnityEvent();

	public void LoadLevel(string level) {
		OnLevelBeginLoad.Invoke();
		SceneManager.LoadSceneAsync(level, LoadSceneMode.Single);
	}
}
