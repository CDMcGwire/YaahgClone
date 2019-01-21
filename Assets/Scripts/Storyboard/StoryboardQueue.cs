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

	/// <summary>General storyboard queue. Orders them in FIFO.</summary>
	private Queue<Storyboard> storyboards = new Queue<Storyboard>();
	private static Queue<Storyboard> Storyboards { get { return Instance.storyboards; } }
	/// <summary>Interrupt storyboard queue. Will drain before allowing standard queue to resume.</summary>
	private Queue<Storyboard> interruptQueue = new Queue<Storyboard>();
	private static Queue<Storyboard> InterruptQueue { get { return Instance.interruptQueue; } }

	/// <summary>Currently playing storyboard.</summary>
	private Storyboard current = null;
	public static Storyboard Current { get { return Instance.current; } private set { Instance.current = value; } }

	private void Start() {
		if (Instance != this) {
			Destroy(gameObject);
			return;
		}
		var existingStoryboards = FindObjectsOfType<Storyboard>();
		foreach (var board in existingStoryboards) EnqueueStoryboard(board);

		foreach (var character in CharacterManager.Characters) {
			if (character.Spawned) HandleCharacterSpawned(character);
		}
		CharacterManager.OnPlayerSpawned.AddListener(HandleCharacterSpawned);
	}

	private void EnqueueStoryboard(Storyboard board, bool interrupt = false) {
		// Trigger switch when current board finishes playing (will start automatically if enabled)
		board.OnBoardFinished.AddListener(PlayNext);

		// If there is no board, this is the current
		if (current == null) {
			board.gameObject.SetActive(true);
			current = board;
		}
		else {
			board.gameObject.SetActive(false);
			if (interrupt) interruptQueue.Enqueue(board);
			else storyboards.Enqueue(board);
		}
	}

	/// <summary>Enqueue the storyboard with a manually specified list of players.</summary>
	/// <param name="storyboardPrefab">The prefab to queue for instantiation.</param>
	/// <param name="owningPlayers">A list of player roster indices.</param>
	/// <param name="interrupt">Whether or not the storyboard should take precedence over any others.</param>
	public static void Enqueue(Storyboard storyboardPrefab, List<int> owningPlayers, bool interrupt = false) {
		if (storyboardPrefab == null) return;

		var board = Instantiate(storyboardPrefab);
		board.OwningPlayers = owningPlayers;
		Instance.EnqueueStoryboard(board, interrupt);
	}

	/// <summary>Enqueue the storyboard with a single specified owner.</summary>
	/// <param name="storyboardPrefab">The prefab to queue for instantiation.</param>
	/// <param name="owningPlayer">The roster index number of the player that should own the board.</param>
	/// <param name="interrupt">Whether or not the storyboard should take precedence over any others.</param>
	public static void Enqueue(Storyboard storyboardPrefab, int owningPlayer, bool interrupt = false) {
		Enqueue(storyboardPrefab, new List<int> { owningPlayer }, interrupt);
	}

	/// <summary>Enqueue the storyboard with all living players by default. If there are none, it will just grab all players.</summary>
	/// <param name="storyboardPrefab">The prefab to queue for instantiation.</param>
	/// <param name="interrupt">Whether or not the storyboard should take precedence over any others.</param>
	public static void Enqueue(Storyboard storyboardPrefab, bool interrupt = false) {
		var livingPlayers = CharacterManager.LivingPlayers;
		Enqueue(storyboardPrefab, livingPlayers.Count > 0 ? livingPlayers : InputManager.AssignedPlayers, interrupt);
	}

	private static void PlayNext() {
		Debug.Log("Playing next storyboard. Queue count: " + Storyboards.Count);
		// Check if processing should complete
		if (Storyboards.Count < 1 && InterruptQueue.Count < 1) {
			Current = null;
			OnQueueCleared.Invoke();
			return;
		}
		Current = InterruptQueue.Count > 0 ? InterruptQueue.Dequeue() : Storyboards.Dequeue();
		Current.gameObject.SetActive(true);
	}

	public void HandleCharacterSpawned(Character character) {
		character.OnDeath.AddListener((c, limits) => Enqueue(genericDeathStoryboard, c.PlayerNumber, true));
	}
}
