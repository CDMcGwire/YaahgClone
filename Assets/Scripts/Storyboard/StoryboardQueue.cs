using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Singleton component that manages Storyboard lifetime and playback.
/// </summary>
[RequireComponent(typeof(StoryboardLoader))]
public class StoryboardQueue : MonoBehaviour {
	private static StoryboardQueue instance;
	public static StoryboardQueue Instance {
		get {
			if (instance == null) instance = FindObjectOfType<StoryboardQueue>();
			return instance;
		}
	}

	[Tooltip("Event fired when all Storyboards in the queue have finished playback.")]
	[SerializeField]
	private UnityEvent onQueueCleared = new UnityEvent();
	public static UnityEvent OnQueueCleared => Instance.onQueueCleared;

	[Tooltip("Default Storyboard to use for DataDriven Encounters.")]
	[SerializeField]
	private DataDrivenBoard defaultDriverBoard;

	[Tooltip("Storyboard to queue upon receiving a player death event.")]
	[SerializeField]
	private Storyboard genericDeathStoryboard;

	private StoryboardLoader storyboardLoader;

	/// <summary>General storyboard queue. Orders them in FIFO.</summary>
	private Queue<LoadableQueueEntry> standardQueue = new Queue<LoadableQueueEntry>();
	private static Queue<LoadableQueueEntry> StandardQueue => Instance.standardQueue;
	/// <summary>Interrupt storyboard queue. Will drain before allowing standard queue to resume.</summary>
	private Queue<LoadableQueueEntry> interruptQueue = new Queue<LoadableQueueEntry>();
	private static Queue<LoadableQueueEntry> InterruptQueue => Instance.interruptQueue;

	private static LoadableQueueEntry DequeueNextEntry() => 
		InterruptQueue.Count > 0 ? InterruptQueue.Dequeue() : 
		StandardQueue.Count > 0 ? StandardQueue.Dequeue() : null;

	/// <summary>Lookup table for currently queued boards to allow for quick merging logic.</summary>
	private Dictionary<string, LoadableQueueEntry> queuedBoards = new Dictionary<string, LoadableQueueEntry>();
	private static Dictionary<string, LoadableQueueEntry> QueuedBoards => Instance.queuedBoards;

	/// <summary>Currently playing storyboard.</summary>
	private Storyboard current = null;
	public static Storyboard Current { get => Instance.current; private set => Instance.current = value; }

	/// <summary>
	/// Tries to find a match a new entry to an existing, mergable entry and combines the owning players if found.
	/// </summary>
	/// <param name="entry">The new entry to attempt to merge.</param>
	/// <returns>True if an entry was found and merged. False if no existing entry found.</returns>
	private bool Merge(LoadableQueueEntry entry) {
		if (queuedBoards.ContainsKey(entry.ID)) {
			queuedBoards[entry.ID].OwningPlayers.UnionWith(entry.OwningPlayers);
			return true;
		}
		else return false;
	}

	private void Start() {
		// Ensure only one instance of the Singleton.
		if (Instance != this) {
			Destroy(gameObject);
			return;
		}
		storyboardLoader = GetComponent<StoryboardLoader>();

		// Queue up any storyboards already exisiting in the scene.
		var existingStoryboards = FindObjectsOfType<Storyboard>();
		foreach (var board in existingStoryboards) Enqueue(board, board.OwningPlayers);
		// Setup existing and new characters to play the death storyboard when they die.
		foreach (var character in CharacterManager.Characters) {
			if (character.Spawned) HandleCharacterSpawned(character);
		}
		CharacterManager.OnPlayerSpawned.AddListener(HandleCharacterSpawned);
	}

	/// <summary>Add a Storyboard to the queue.</summary>
	/// <param name="entry">GameObject with Storyboard component.</param>
	/// <param name="interrupt">Should this board take priority over normal boards?</param>
	/// <param name="merge">Should the owning players be merged if a matching board has been queued?</param>
	private void EnqueueEntry(LoadableQueueEntry entry, bool interrupt = false, bool merge = true) {
		if (entry == null || entry.Failed) Debug.LogWarning("Tried to enqueue entry but it was either null or failed to load.");
		// Check if a board with the same ID has been queued. Merge the owning players if found.
		else if (merge && Instance.Merge(entry)) return;
		// If there is no board, this is the current one. Play immediately if ready.
		else if (current == null) {
			if (entry.Loaded) PlayEntry(entry);
			else entry.OnEntryLoaded.AddListener(() => PlayEntry(entry));
		}
		else {
			if (interrupt) interruptQueue.Enqueue(entry);
			else standardQueue.Enqueue(entry);
			queuedBoards[entry.ID] = entry;
		}
	}

	/// <summary>Enqueue the storyboard with a manually specified list of players.</summary>
	/// <param name="storyboardPrefab">The prefab to queue for instantiation.</param>
	/// <param name="interrupt">Should this board take priority over normal boards?</param>
	/// <param name="merge">Should the owning players be merged if a matching board has been queued?</param>
	public static void Enqueue(Storyboard storyboardPrefab, IEnumerable<int> owningPlayers, bool interrupt = false, bool merge = true) {
		if (storyboardPrefab == null) return;

		var entry = new PrefabQueueEntry(storyboardPrefab);
		entry.OwningPlayers.UnionWith(owningPlayers);
		Instance.EnqueueEntry(entry, interrupt, merge);
	}

	/// <summary>Enqueue the storyboard with a single specified owner.</summary>
	/// <param name="storyboardPrefab">The prefab to queue for instantiation.</param>
	/// <param name="owningPlayer">The roster index number of the player that should own the board.</param>
	/// <param name="interrupt">Whether or not the storyboard should take precedence over any others.</param>
	public static void Enqueue(Storyboard storyboardPrefab, int owningPlayer, bool interrupt = false, bool merge = true) =>
		Enqueue(storyboardPrefab, new List<int> { owningPlayer }, interrupt, merge);

	/// <summary>Enqueue the storyboard with all living players by default. If there are none, it will just grab all players.</summary>
	/// <param name="storyboardPrefab">The prefab to queue for instantiation.</param>
	/// <param name="interrupt">Whether or not the storyboard should take precedence over any others.</param>
	public static void Enqueue(Storyboard storyboardPrefab, bool interrupt = false, bool merge = true) {
		var livingPlayers = CharacterManager.LivingPlayers;
		Enqueue(storyboardPrefab, livingPlayers.Count > 0 ? livingPlayers : InputManager.AssignedPlayers, interrupt, merge);
	}

	/// <summary>
	/// Enqueue a storyboard according to its URI in the game data. This may be either in
	/// a loadable data based storyboard or a custom prefab based storyboard. Uri is of form
	/// {AssetType}:{BundlePath}/{StoryboardName}
	/// </summary>
	/// <param name="storyboardUri">Path to the storyboard.</param>
	/// <param name="owningPlayers">A list of player roster indices.</param>
	/// <param name="interrupt">Should this board take priority over normal boards?</param>
	/// <param name="merge">Should the owning players be merged if a matching board has been queued?</param>
	public static void Enqueue(string storyboardUri, IEnumerable<int> owningPlayers, bool interrupt = false, bool merge = true) {
		var uriSplit = storyboardUri.Split(":".ToCharArray(), 2);
		var scheme = uriSplit[0];

		LoadableQueueEntry entry = null;
		if (scheme.ToLower() == "data") {
			var dataEntry = new DataQueueEntry();
			Instance.storyboardLoader.LoadData(
				uriSplit[1], 
				data => dataEntry.Data = data
			);
			entry = dataEntry;
		}
		else if (scheme.ToLower() == "prefab") {
			var prefabEntry = new PrefabQueueEntry();
			Instance.storyboardLoader.LoadPrefab(
				uriSplit[1], 
				prefab => prefabEntry.Prefab = prefab
			);
			entry = prefabEntry;
		}
		else Debug.LogWarning($"Tried to enqueue Storyboard from URI {storyboardUri} but no scheme was given to determine the resource type.");

		if (entry != null) {
			entry.OwningPlayers.UnionWith(owningPlayers);
			Instance.EnqueueEntry(entry, interrupt, merge);
		}
	}

	/// <summary>
	/// Enqueue a storyboard according to its URI in the game data. This may be either in
	/// a loadable data based storyboard or a custom prefab based storyboard. Uri is of form
	/// {AssetType}:{BundlePath}/{StoryboardName}
	/// </summary>
	/// <param name="storyboardUri">Path to the storyboard.</param>
	/// <param name="owningPlayer">The roster index number of the player that should own the board.</param>
	/// <param name="interrupt">Whether or not the storyboard should take precedence over any others.</param>
	public static void Enqueue(string storyboardUri, int owningPlayer, bool interrupt = false, bool merge = true) => 
		Enqueue(storyboardUri, new List<int> { owningPlayer }, interrupt, merge);

	/// <summary>
	/// Enqueue a storyboard according to its URI in the game data. This may be either in
	/// a loadable data based storyboard or a custom prefab based storyboard. Uri is of form
	/// {AssetType}:{BundlePath}/{StoryboardName}
	/// </summary>
	/// <param name="storyboardUri">Path to the storyboard.</param>
	/// <param name="interrupt">Whether or not the storyboard should take precedence over any others.</param>
	public static void Enqueue(string storyboardUri, bool interrupt = false, bool merge = true) {
		var livingPlayers = CharacterManager.LivingPlayers;
		Enqueue(storyboardUri, livingPlayers.Count > 0 ? livingPlayers : InputManager.AssignedPlayers, interrupt, merge);
	}

	/// <summary>Callback to process the next entry in the queue, or invoke OnQueueCleared if the queue is finished.</summary>
	private static void HandleNext() {
		Current.OnBoardFinished.RemoveListener(HandleNext);
		_ = Instance.StartCoroutine(Instance.DelayedHandleNext());
	}

	/// <summary>
	/// HandleNext logic is handled as a coroutine to give the last storyboard a chance to finish all of its
	/// Monobehaviour methods for the frame.
	/// </summary>
	/// <returns>A single frame Enumerator.</returns>
	private IEnumerator DelayedHandleNext() {
		yield return null;
		var entry = DequeueNextEntry();
		while (entry != null && entry.Failed) {
			Debug.LogWarning($"Dequeueing Storyboard entry as it failed to load.");
			entry = DequeueNextEntry();
		}
		// Check if processing should complete
		if (entry == null) {
			Debug.Log("Storyboard Queue has been cleared.");
			Current = null;
			OnQueueCleared.Invoke();
			yield break;
		}
		else {
			// Entry loaded while queued, play immediately
			if (entry.Loaded) PlayEntry(entry);
			// Next entry has not loaded yet, set a callback to retry when loaded
			else entry.OnEntryLoaded.AddListener(() => PlayEntry(entry));
		}
	}

	/// <summary>
	/// Retrieves the next storyboard to play from the given entry, sets it as the current, and enables it.
	/// </summary>
	/// <param name="entry">The entry to play.</param>
	private static void PlayEntry(LoadableQueueEntry entry) {
		Debug.Log($"Playing next storyboard. Queue count: {StandardQueue.Count}; Interrupt Count: {InterruptQueue.Count}");
		Current = GetEntryStoryboard(entry);
		// Setup next board to play when current board finishes
		Current.OnBoardFinished.AddListener(HandleNext);
		_ = QueuedBoards.Remove(entry.ID);
		Current.Play();
	}

	/// <summary>
	/// Gets a playable storyboard from an entry, performing any general and type specific initialization required.
	/// </summary>
	/// <param name="entry">The entry to turn into a playable storyboard.</param>
	/// <returns>A playable storyboard.</returns>
	private static Storyboard GetEntryStoryboard(LoadableQueueEntry entry) {
		Storyboard board;
		if (entry is PrefabQueueEntry) {
			var prefabEntry = entry as PrefabQueueEntry;
			board = Instantiate(prefabEntry.Prefab);
		}
		else if (entry is DataQueueEntry) {
			var dataEntry = entry as DataQueueEntry;
			Instance.defaultDriverBoard.StoryboardData = dataEntry.Data;
			board = Instance.defaultDriverBoard;
		}
		else {
			Debug.LogError("Created an entry which cannot produce a storyboard.");
			return null;
		}

		board.OwningPlayers = new List<int>(entry.OwningPlayers);
		return board;
	}

	/// <summary>
	/// Ensure that a DeathStoryboard is queued when a player is killed.
	/// </summary>
	/// <param name="character"></param>
	public void HandleCharacterSpawned(Character character) => character.OnDeath.AddListener(
		(c, limits) => Enqueue(genericDeathStoryboard, c.PlayerNumber, true)
	);

	/// <summary>Base queueable entry.</summary>
	private abstract class LoadableQueueEntry {
		/// <summary>The identifying string for the entry. Not guranteed unique.</summary>
		public abstract string ID { get; }

		/// <summary>Flag indicating if the entry has successfully loaded.</summary>
		private LoadState state = LoadState.Loading;

		/// <summary>
		/// True if the entry's data is available, else false. Triggers and clear OnEntryLoaded when set to true.
		/// Should be handled by the subtype and set when the sub type receives all necessary data.
		/// </summary>
		public LoadState State {
			get => state;
			protected set {
				state = value;
				if (state != LoadState.Loading) {
					OnEntryLoaded.Invoke();
					OnEntryLoaded.RemoveAllListeners();
				}
			}
		}

		/// <summary>True if the value has successfully loaded.</summary>
		public bool Loaded => State == LoadState.Success;
		/// <summary>True if the value has failed to load.</summary>
		public bool Failed => State == LoadState.Failure;

		/// <summary>The players that should own the resulting storyboard.</summary>
		public HashSet<int> OwningPlayers { get; } = new HashSet<int>();
		/// <summary>Event fired when the entry's data is all available or loading failed.</summary>
		public UnityEvent OnEntryLoaded { get; } = new UnityEvent();

		public enum LoadState {
			Loading,
			Success,
			Failure
		}
	}

	/// <summary>Queue entry for data based storyboards.</summary>
	private class DataQueueEntry : LoadableQueueEntry {
		private StoryboardData data = null;
		public StoryboardData Data {
			get => data;
			set { data = value; State = data == null ? LoadState.Failure : LoadState.Success; }
		}

		public override string ID => data?.ID ?? "None";
	}

	/// <summary>Queue entry for prefab based storyboards.</summary>
	private class PrefabQueueEntry : LoadableQueueEntry {
		private Storyboard prefab;
		public Storyboard Prefab {
			get => prefab;
			set { prefab = value; State = prefab == null ? LoadState.Failure : LoadState.Success; }
		}

		public PrefabQueueEntry() { }
		public PrefabQueueEntry(Storyboard board) => prefab = board;

		public override string ID => prefab?.ID ?? "None";
	}
}
