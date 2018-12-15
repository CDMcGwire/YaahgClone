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

	private SortedList<int, PlayerChoice> playerChoices = new SortedList<int, PlayerChoice>(new DecisionComparer());

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
		
		foreach (var decision in decisions) {
			var button = Instantiate(buttonPrefab, buttonParent);
			button.onClick.AddListener(() => {
				// Decision ordering is derived from the base ordering minus the choosing character's dex and perception; Lower is sooner
				var player = players[currentPlayerIndex];
				var playerCharacter = CharacterManager.GetLivingCharacter(player);
				var decisionOrder = decision.PlaybackOrder - playerCharacter.Data.Dexterity - playerCharacter.Data.Perception;
				playerChoices.Add(decisionOrder, new PlayerChoice(player, decision));

				currentPlayerIndex++;
				onDecisionMade.Invoke();
			});

			var buttonText = button.GetComponentInChildren<Text>();
			if (buttonText) {
				buttonText.text = decision.OptionText;
			}
			else Debug.LogWarning("Button prefab for decision control is missing a Text component");
		}

		onDecisionMade.AddListener(() => {
			if (currentPlayerIndex < players.Count) decisionMenu.Cycle();
			else decisionMenu.Close();
		});

		players = Panel.Board.OwningPlayers;
		currentPlayerIndex = 0;
		Refresh();
	}

	// Refreshes the display of the control if another player needs to choose, else invokes the onDecisionMade event
	public void Refresh() {
		if (currentPlayerIndex >= players.Count) {
			foreach (var choice in playerChoices.Values) StoryboardQueue.Add(choice.Decision.StoryboardPrefab, choice.Player);
			if (summaryStoryboard) StoryboardQueue.Add(summaryStoryboard);
			onAllDecisionsMade.Invoke();
			return;
		}

		var character = CharacterManager.GetLivingCharacter(currentPlayerIndex);
		playerPrompt.text = string.Format(playerPromptFormat, character.Data.Firstname);
		playerPrompt.color = character.Data.Color;
	}

	// Stores a decision with the choosing player for use in sorting
	private struct PlayerChoice {
		public int Player { get; }
		public Decision Decision { get; }

		public PlayerChoice(int player, Decision decision) {
			Player = player;
			Decision = decision;
		}
	}
}

[Serializable]
public struct Decision {
	/// <summary>
	/// OptionText displays on the button and should tell the player what they are doing.
	/// </summary>
	public string OptionText;
	/// <summary>
	/// PlaybackOrder determines the base relative timing the storyboards should playback in.
	/// </summary>
	public int PlaybackOrder;
	/// <summary>
	/// The Storyboard Prefab that should be instantiated for this decision.
	/// </summary>
	public Storyboard StoryboardPrefab;
}

public class DecisionComparer : IComparer<int> {
	public int Compare(int x, int y) {
		var result = x.CompareTo(y);
		return result == 0 ? -1 : result;
	}
}