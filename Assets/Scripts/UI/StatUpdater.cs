using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatUpdater : MonoBehaviour {
	private static StatUpdater instance;
	public static StatUpdater Instance {
		get {
			if (instance == null) instance = FindObjectOfType<StatUpdater>();
			return instance;
		}
	}

	[SerializeField]
	private Menu menu;

	[SerializeField]
	private Text nameDisplay;
	[SerializeField]
	private Text titleDisplay;

	[SerializeField]
	private float updatesPerSecond = 3;

	private Dictionary<CharacterStat, StatDisplay> displays = new Dictionary<CharacterStat, StatDisplay>();

	private Queue<UpdateInfo> updateQueue = new Queue<UpdateInfo>();
	private UpdateInfo currentUpdate;

	private float timer = 0f;
	private bool CanUpdate { get { return updatesPerSecond > 0 ? timer >= 1 / updatesPerSecond : true; } }

	private void OnValidate() {
		Debug.Assert(menu, $"State updater '{name}' is missing a Menu component reference");
		Debug.Assert(nameDisplay, $"State updater '{name}' is missing a Text component reference for character name display");
		Debug.Assert(titleDisplay, $"State updater '{name}' is missing a Text component reference for character title display");
	}

	private void Awake() {
		var statDisplays = GetComponentsInChildren<StatDisplay>();
		foreach (var statDisplay in statDisplays) displays[statDisplay.Stat] = statDisplay;
	}

	private void Start() {
		foreach (var character in CharacterManager.Characters) character.OnStatChange.AddListener(EnqueueUpdate);
		// If a new character is spawned, listen to that one as well.
		CharacterManager.OnPlayerSpawned.AddListener(character => character.OnStatChange.AddListener(EnqueueUpdate));

		// Start disabled
		enabled = false;
		menu.gameObject.SetActive(false);
	}

	private void Update() {
		if (updateQueue.Count < 1) return;

		timer += Time.deltaTime;
		if (CanUpdate) {
			var current = updateQueue.Peek();

			var node = current.changes.First;
			while (node != null) {
				var next = node.Next;

				var change = node.Value;
				var display = displays[change.stat];
				var targetValue = current.originalState.GetStat(change.stat) + change.value;

				if (change.stat == CharacterStat.MONEY) display.Set(targetValue);
				else if (display.Value < targetValue) display.Increment();
				else if (display.Value > targetValue) display.Decrement();
				else current.changes.Remove(node);

				node = next;
			}
			if (current.changes.Count < 1) {
				updateQueue.Dequeue();
				if (updateQueue.Count < 1) menu.Close();
				else menu.Cycle();
			}

			timer = 0;
		}
	}

	public void EnqueueUpdate(CharacterState originalState, List<StatChange> changes) {
		updateQueue.Enqueue(new UpdateInfo(originalState, changes));
		menu.gameObject.SetActive(true);
	}

	public void RefreshDisplay() {
		if (updateQueue.Count < 1) return;

		var character = updateQueue.Peek().originalState;
		nameDisplay.text = character.name;
		nameDisplay.color = character.color;
		titleDisplay.text = character.title;
		titleDisplay.color = character.color;

		foreach (var entry in displays) entry.Value.Value = character.GetStat(entry.Key);
	}

	private struct UpdateInfo {
		public readonly CharacterState originalState;
		public readonly LinkedList<StatChange> changes;

		public UpdateInfo(CharacterState originalState, List<StatChange> changes) {
			this.originalState = originalState;
			this.changes = new LinkedList<StatChange>(changes);
		}
	}
}
