using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DecisionControl : PanelControl {
	[SerializeField]
	private UnityEvent onDecisionMade;
	public UnityEvent OnDecisionMade { get { return onDecisionMade; } }

	[SerializeField]
	private UnityEvent onAllDecisionsMade;
	public UnityEvent OnAllDecisionsMade { get { return onAllDecisionsMade; } }

	[SerializeField]
	private List<Decision> decisions = new List<Decision>();

	/// <summary>If true, players who pick the same option will be grouped in the resulting storyboard.</summary>
	[SerializeField]
	private bool combineChoices = true;

	[SerializeField]
	private Storyboard summaryStoryboard;

	[SerializeField]
	private Menu decisionMenu;

	[SerializeField]
	private Text playerPrompt;

	[SerializeField]
	private string playerPromptFormat = "{0} - Whadya' do?";

	[SerializeField]
	private Button buttonPrefab;

	[SerializeField]
	private Transform buttonParent;

	private List<int> players;
	private int currentPlayerIndex = 0;

	/// <summary>Keeps UI input from triggering until the menu is ready to accept it.</summary>
	private bool ready = false;

	private Dictionary<string, Choice> playerChoices = new Dictionary<string, Choice>();

	public bool isInvalid {
		get {
			if (!buttonPrefab) {
				Debug.LogError("Missing a button prefab for Decision control component");
				return true;
			}
			if (!decisionMenu) {
				Debug.LogError("Missing a button prefab for Decision control component");
				return true;
			}
			if (Panel.Board.OwningPlayers.Count < 1) {
				Debug.LogError("Decision Controller initialization was attempted with no owning players");
				return true;
			}
			return false;
		}
	}

	protected override void Init() {
		if (isInvalid) return;

		if (buttonParent == null) buttonParent = transform;
		foreach (Transform child in buttonParent) Destroy(child.gameObject);

		Button firstButton = null;

		// When the menu is fully opened, enable input.
		decisionMenu.OnOpened.AddListener(() => ready = true);

		foreach (var decision in decisions) {
			var button = Instantiate(buttonPrefab, buttonParent);
			if (firstButton == null) firstButton = button;

			button.onClick.AddListener(() => {
				if (!ready) return;
				if (decision.StoryboardPrefab != null) {
					// Add player to a choice group, or instantiate if first in the group
					var player = players[currentPlayerIndex];
					var storyboard = decision.StoryboardPrefab.name;
					if (playerChoices.ContainsKey(storyboard)) {
						playerChoices[storyboard].Players.Add(player);
					}
					else playerChoices[storyboard] = new Choice(player, decision);
				}

				// Trigger event and prepare for next input.
				currentPlayerIndex++;
				ready = false;
				onDecisionMade.Invoke();
			});

			// If button has a Selection Trigger component, daisy chain the invocations
			var selectionTrigger = button.GetComponent<SelectionTrigger>();
			if (selectionTrigger != null) {
				selectionTrigger.onSelect.AddListener(() => decision.Events.OnSelect.Invoke());
				selectionTrigger.onDeselect.AddListener(() => decision.Events.OnDeselect.Invoke());
			}

			// Set the prompt text of the button, if present
			var buttonText = button.GetComponentInChildren<Text>();
			if (buttonText) buttonText.text = decision.OptionText;
		}

		onDecisionMade.AddListener(() => {
			if (currentPlayerIndex < players.Count) decisionMenu.Cycle();
			else decisionMenu.Close();
		});

		var selector = GetComponent<SelectOnEnable>();
		if (selector != null) selector.TargetSelectable = firstButton;

		players = Panel.Board.OwningPlayers;
		currentPlayerIndex = 0;
		Refresh();
	}

	/// <summary>
	/// Refreshes the display of the control if another player needs to choose, 
	/// else invokes the onDecisionMade event.
	/// </summary>
	public void Refresh() {
		if (currentPlayerIndex >= players.Count) {
			QueueChoices();
			return;
		}

		var player = Panel.Board.OwningPlayers[currentPlayerIndex];
		var character = CharacterManager.GetCharacter(player);
		playerPrompt.text = string.Format(playerPromptFormat, character.Data.Firstname);
		playerPrompt.color = character.Data.Color;
	}

	/// <summary>Sorts and optionally groups the player choices then pushes the associated storyboard to the queue.</summary>
	private void QueueChoices() {
		if (combineChoices) {
			// Create sorted list of group decisions
			var sortedChoices = new SortedList<int, Choice>(new DecisionComparer());
			foreach (var choice in playerChoices.Values) {
				sortedChoices.Add(choice.CalculatePlaybackOrder(), choice);
			}
			// Enqueue storyboards from sorted list
			foreach (var choice in sortedChoices.Values) {
				StoryboardQueue.Enqueue(choice.Decision.StoryboardPrefab, choice.Players);
			}
		}
		else {
			var sortedStoryboards = new SortedList<int, Choice>(new DecisionComparer());
			// Sort choices
			foreach (var choice in playerChoices.Values) {
				foreach (var player in choice.Players) {
					var storyboard = choice.Decision.StoryboardPrefab;
					var playerChoice = new Choice(player, choice.Decision);
					sortedStoryboards.Add(choice.CalculatePlaybackOrder(), playerChoice);
				}
			}
			// Queue choices in order
			foreach (var choice in sortedStoryboards.Values) {
				StoryboardQueue.Enqueue(choice.Decision.StoryboardPrefab, choice.Players);
			}
		}
		if (summaryStoryboard) StoryboardQueue.Enqueue(summaryStoryboard);
		onAllDecisionsMade.Invoke();
		return;
	}

	/// <summary>Stores a decision along a list of players that made it.</summary>
	private class Choice {
		public List<int> Players { get; }
		public Decision Decision { get; }

		public Choice(int player, Decision decision) {
			Players = new List<int> { player };
			Decision = decision;
		}
		public Choice(List<int> players, Decision decision) {
			Players = players;
			Decision = decision;
		}

		/// <summary>Calculates playback order using group initiative.</summary>
		public int CalculatePlaybackOrder() {
			// Average group initiative and add group size bonus
			var sum = 0;
			foreach (var player in Players) {
				var character = CharacterManager.GetCharacter(player);
				sum += character.Data.Initiative;
			}
			var baseOrder = Decision.PlaybackOrder;
			var groupSize = Players.Count;
			var initiative = Mathf.RoundToInt(sum / (float)groupSize) + (groupSize - 1);

			// Subtract initiative from base decision order and return value
			return Decision.PlaybackOrder - initiative;
		}
	}
}

[Serializable]
public class Decision {
	/// <summary>OptionText displays on the button and should tell the player what they are doing.</summary>
	public string OptionText;
	/// <summary>PlaybackOrder determines the base relative timing the storyboards should playback in.</summary>
	public int PlaybackOrder;
	/// <summary>The Storyboard Prefab that should be instantiated for this decision.</summary>
	public Storyboard StoryboardPrefab;
	/// <summary>Struct to hold event references so a collapsable region in the inspector.</summary>
	public EventContainer Events = new EventContainer(new UnityEvent(), new UnityEvent());

	[Serializable]
	public struct EventContainer {
		/// <summary>Event to fire when button is selected.</summary>
		public UnityEvent OnSelect;
		/// <summary>Event to fire when button is no longer selected.</summary>
		public UnityEvent OnDeselect;

		public EventContainer(UnityEvent onSelect, UnityEvent onDeselect) {
			OnSelect = onSelect;
			OnDeselect = onDeselect;
		}
	}
}

/// <summary>IComparer implementation for sorting decision objects.</summary>
public class DecisionComparer : IComparer<int> {
	public int Compare(int x, int y) {
		var result = x.CompareTo(y);
		return result == 0 ? -1 : result;
	}
}