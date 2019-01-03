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

	private void Start() {
		var existingStoryboards = FindObjectsOfType<Storyboard>();
		foreach (var board in existingStoryboards) EnqueueStoryboard(board);
	}

	private void EnqueueStoryboard(Storyboard board) {
		board.OnBoardFinished.AddListener(PlayNext);
		if (storyboards.Count > 0) board.gameObject.SetActive(false);
		storyboards.Enqueue(board);
	}

	public static void Add(Storyboard storyboardPrefab, List<int> owningPlayers) {
		if (storyboardPrefab == null) return;

		var board = Instantiate(storyboardPrefab);
		board.OwningPlayers = owningPlayers;
		Instance.EnqueueStoryboard(board);
	}

	public static void Add(Storyboard storyboardPrefab, int owningPlayer) {
		Add(storyboardPrefab, new List<int> { owningPlayer });
	}

	public static void Add(Storyboard storyboardPrefab) {
		Add(storyboardPrefab, CharacterManager.GetLivingPlayers());
	}

	private static void PlayNext() {
		Instance.storyboards.Dequeue(); // Remove calling storyboard from queue
		Debug.Log("Playing next storyboard. Queue count: " + Instance.storyboards.Count);
		if (Instance.storyboards.Count < 1) {
			OnQueueCleared.Invoke();
			return;
		}
		var next = Instance.storyboards.Peek();
		next.gameObject.SetActive(true);
	}

	public void HandleCharacterSpawned(Character character) {
		character.OnDeath.AddListener((c, limits) => Add(genericDeathStoryboard, c.PlayerNumber));
	}
}
