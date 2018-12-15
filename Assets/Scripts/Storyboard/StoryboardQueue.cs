using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StoryboardQueue : MonoBehaviour {
	private static StoryboardQueue instance;
	public static StoryboardQueue Instance {
		get {
			if (instance == null) instance = FindObjectOfType<StoryboardQueue>();
			return instance;
		}
	}

	[SerializeField]
	private UnityEvent onQueueCleared = new UnityEvent();
	public static UnityEvent OnQueueCleared { get { return Instance.onQueueCleared; } }

	[SerializeField]
	private Storyboard genericDeathStoryboard;

	private Queue<Storyboard> storyboards = new Queue<Storyboard>();

	public static void Add(Storyboard storyboardPrefab, List<int> owningPlayers) {
		if (storyboardPrefab == null) return;

		if (storyboardPrefab.gameObject.scene.rootCount != 0) {
			Debug.LogError("Tried to add a non-prefab storyboard to the queue");
			return;
		}
		var storyboard = Instantiate(storyboardPrefab);

		storyboard.OwningPlayers = owningPlayers;
		storyboard.OnBoardFinished.AddListener(PlayNext);
		Instance.storyboards.Enqueue(storyboard);

		if (Instance.storyboards.Count > 1) storyboard.gameObject.SetActive(false);
	}

	public static void Add(Storyboard storyboardPrefab, int owningPlayer) {
		Add(storyboardPrefab, new List<int> { owningPlayer });
	}

	public static void Add(Storyboard storyboardPrefab) {
		Add(storyboardPrefab, CharacterManager.GetLivingPlayers());
	}

	private static void PlayNext() {
		if (Instance.storyboards.Count < 1) {
			OnQueueCleared.Invoke();
			return;
		}
		var next = Instance.storyboards.Dequeue();
		next.gameObject.SetActive(true);
	}

	public void HandleCharacterSpawned(Character character) {
		character.OnDeath.AddListener((c, limits) => Add(genericDeathStoryboard, c.PlayerNumber));
	}
}
