using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton component which manages and provides an interface for the Stat Updater UI.
/// </summary>
public class StatUpdateDisplay : MonoBehaviour {
	private static StatUpdateDisplay instance;
	public static StatUpdateDisplay Instance {
		get {
			if (instance == null) instance = FindObjectOfType<StatUpdateDisplay>();
			return instance;
		}
	}

	[Tooltip("How long the menu should hold open after finishing its update.")]
	[SerializeField]
	private float holdTime = 1f;

	[Tooltip("How many ticks the display should update by in a second.")]
	[SerializeField]
	private float updatesPerSecond = 3f;

	[Tooltip("Should the next update happen without a tick delay?")]
	[SerializeField]
	private bool fastForward = false;

	[Space(12)]

	[Tooltip("Reference to the display's Menu component.")]
	[SerializeField]
	private Menu menu;

	[Tooltip("Reference to the Text component that should be used to display the updating character's name.")]
	[SerializeField]
	private Text nameDisplay;

	[Tooltip("Reference to the Text component that should be used to display the updating character's title.")]
	[SerializeField]
	private Text titleDisplay;

	/// <summary>The set of game objects that will be used to display each stat.</summary>
	private Dictionary<CharacterStat, StatUpdateField> fields = new Dictionary<CharacterStat, StatUpdateField>();

	/// <summary>The queue of character updates that need to be displayed.</summary>
	private Queue<UpdateInfo> updateQueue = new Queue<UpdateInfo>();

	/// <summary>Tracks the current time passed sinced the current update started.</summary>
	private float timer = 0f;

	/// <summary>True if enough time has passed since the last tick to update.</summary>
	private bool CanUpdate => updatesPerSecond > 0 ? timer >= 1 / updatesPerSecond : true;

	/// <summary>Key to use for pause requests</summary>
	private readonly string pauseKey = "StatUpdater";

	private void OnValidate() {
		Debug.Assert(menu, $"State updater '{name}' is missing a Menu component reference");
		Debug.Assert(nameDisplay, $"State updater '{name}' is missing a Text component reference for character name display");
		Debug.Assert(titleDisplay, $"State updater '{name}' is missing a Text component reference for character title display");
	}

	private void Awake() {
		var statDisplays = GetComponentsInChildren<StatUpdateField>();
		foreach (var statDisplay in statDisplays) fields[statDisplay.Stat] = statDisplay;
	}

	private void Start() {
		if (Instance != this) {
			Destroy(gameObject);
			return;
		}

		// Check for pre-spawned characters
		foreach (var character in CharacterManager.Characters) {
			if (character.Spawned) character.OnStatChange.AddListener(EnqueueUpdate);
		}
		// If a new character is spawned, listen to that one as well.
		CharacterManager.OnPlayerSpawned.AddListener(character => character.OnStatChange.AddListener(EnqueueUpdate));

		// Start disabled
		enabled = false;
		menu.gameObject.SetActive(false);
	}

	private void Update() {
		if (updateQueue.Count < 1) return;

		timer += Time.unscaledDeltaTime;
		if (fastForward || CanUpdate) {
			var current = updateQueue.Peek();

			var node = current.changes.First;
			while (node != null) {
				var next = node.Next;

				var change = node.Value;
				var display = fields[change.stat];
				var targetValue = current.originalState.GetStat(change.stat) + change.value;

				if (fastForward || change.stat == CharacterStat.MONEY) {
					display.Set(targetValue);
					current.changes.Remove(node);
				}
				else if (display.Value < targetValue) display.Increment();
				else if (display.Value > targetValue) display.Decrement();
				else current.changes.Remove(node);

				node = next;
			}
			if (current.changes.Count < 1) {
				updateQueue.Dequeue();
				StartCoroutine(Close(holdTime));
				enabled = false;
			}
			timer = 0;
		}
	}

	/// <summary>Add a Character State update to the queue. Will be played one at a time.</summary>
	/// <param name="originalState">The character's state before the change.</param>
	/// <param name="changes">A list of stats that are updating with their delta values.</param>
	public void EnqueueUpdate(CharacterState originalState, List<StatChange> changes) {
		updateQueue.Enqueue(new UpdateInfo(originalState, changes));
		menu.Open();
		Playback.RequestPause(pauseKey);
	}

	public void RefreshDisplay() {
		if (updateQueue.Count < 1) return;

		var character = updateQueue.Peek().originalState;
		nameDisplay.text = character.name;
		nameDisplay.color = character.color;
		titleDisplay.text = "the " + character.title;
		titleDisplay.color = character.color;

		foreach (var entry in fields) entry.Value.Value = character.GetStat(entry.Key);
	}

	/// <summary>Triggers the menu to immediately show the final values and begin to close.</summary>
	public void FastForward() {
		fastForward = true;
	}

	/// <summary>Coroutine to close the Stat Updater after some amount of unscaled time.</summary>
	/// <param name="time">Time to wait.</param>
	/// <returns>IEnumerator for the Coroutine.</returns>
	private IEnumerator Close(float time) {
		yield return new WaitForSecondsRealtime(time);

		fastForward = false;
		if (updateQueue.Count < 1) {
			Playback.ReleasePause(pauseKey);
			menu.Close();
		}
		else menu.Cycle();
	}

	/// <summary>Struct to track infomation about what stats should display updates.</summary>
	private struct UpdateInfo {
		public readonly CharacterState originalState;
		public readonly LinkedList<StatChange> changes;

		public UpdateInfo(CharacterState originalState, List<StatChange> changes) {
			this.originalState = originalState;
			this.changes = new LinkedList<StatChange>(changes);
		}
	}
}
