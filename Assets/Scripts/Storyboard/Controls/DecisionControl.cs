using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Panel Control that allows for user input between a set of options.
/// </summary>
public class DecisionControl : PanelControl {
	[Tooltip("Text to display as the context of the choice.")]
	[SerializeField]
	[TextArea(1, 2)]
	private string choiceContext = "";
	public string ChoiceContext { get => choiceContext; set => choiceContext = value ?? ""; }

	[Tooltip("Format to use for the player name prompt. Where {0} is replaced with the player's name.")]
	[SerializeField]
	private string playerPromptFormat = "{0} - Whadya' do?";
	public string PlayerPromptFormat { get => playerPromptFormat; set => playerPromptFormat = value ?? ""; }

	[Tooltip("The set of possible decisions for each player.")]
	[SerializeField]
	private List<DecisionControlData> decisions = new List<DecisionControlData>();
	public List<DecisionControlData> Decisions { get => decisions; set => decisions = value ?? new List<DecisionControlData>(); }

	[Tooltip("If true, players who pick the same option will be grouped in the resulting storyboard.")]
	[SerializeField]
	private bool combineChoices = true;

	[Tooltip("Event that fires after each player selects an option.")]
	[SerializeField]
	private UnityEvent onDecisionMade;
	public UnityEvent OnDecisionMade => onDecisionMade;

	[Tooltip("Event that fires once all players have finished making their decisions.")]
	[SerializeField]
	private UnityEvent onAllDecisionsMade;
	public UnityEvent OnAllDecisionsMade => onAllDecisionsMade;

	[Tooltip("Reference to the Decision UI 'Menu' Component.")]
	[SerializeField]
	private Menu decisionMenu;

	[Tooltip("Reference to the 'Text' Component that should be used for displaying the context.")]
	[SerializeField]
	private Text contextDisplay;

	[Tooltip("Reference to the 'Text' Component that should be used for displaying the player prompt.")]
	[SerializeField]
	private Text playerPrompt;

	[Tooltip("Button Prefab to use for generating the options list.")]
	[SerializeField]
	private Button buttonPrefab;

	[Tooltip("Reference to the Transform that the option buttons should be parented to.")]
	[SerializeField]
	private Transform buttonParent;

	/// <summary>Index of the currently selecting player on the Owners list.</summary>
	private int currentPlayerIndex = 0;
	private int CurrentPlayer => Panel.Board.OwningPlayers[currentPlayerIndex];

	/// <summary>Keeps UI input from triggering until the menu is ready to accept it.</summary>
	private bool ready = false;

	/// <summary>Maintain a map of buttons to decision info, so they can be condition checked on each referesh.</summary>
	private readonly Dictionary<string, Button> decisionButtons = new Dictionary<string, Button>();

	/// <summary>Maintains the player choices until they are ready to be fired off.</summary>
	private readonly NarrativeFork fork = new NarrativeFork();

	private void OnValidate() {
		// If editing the game, update the text prompts to match what's on the component
		if (!Application.isPlaying) {
			if (contextDisplay != null) contextDisplay.text = choiceContext;
			if (playerPrompt != null) playerPrompt.text = string.Format(playerPromptFormat, "Johnathan");
		}
	}

	public bool IsInvalid {
		get {
			if (buttonPrefab == null) {
				Debug.LogError("Missing a button prefab for Decision control component");
				return true;
			}
			if (decisionMenu == null) {
				Debug.LogError("Missing a button prefab for Decision control component");
				return true;
			}
			if (Panel.Board.OwningPlayers.Count < 1) {
				Debug.LogError("Decision Controller initialization was attempted with no owning players");
				return true;
			}
			if (Decisions.Count < 1) {
				Debug.LogWarning($"Decision Controller on {name} was initialized but had no decisions.");
				return true;
			}
			return false;
		}
	}

	protected override void Init() {
		// If improperly configured, end control immediately.
		if (IsInvalid) {
			EndControl();
			return;
		}

		// Prepare button list.
		if (buttonParent == null) buttonParent = transform;
		foreach (Transform child in buttonParent) Destroy(child.gameObject);
		decisionButtons.Clear();

		// Note: Could have a list of static buttons that, if populated, stops the dynamic button behaviour.
		//       The static buttons could be linked to decisions on the list by an ID. But it mandates that all
		//       present decisions have a linked button.

		Button firstButton = null;

		// When the menu is fully opened, enable input.
		decisionMenu.OnOpened.AddListener(() => ready = true);

		// Advance the menu when a player makes their selection.
		OnDecisionMade.AddListener(Advance);

		foreach (var decision in decisions) {
			// Create a button for the decision 
			var button = Instantiate(buttonPrefab, buttonParent);

			if (firstButton == null) firstButton = button;

			button.onClick.AddListener(() => {
				if (!ready) return;
				Select(decision);
				ready = false;
			});

			// If button has a Selection Trigger component, daisy chain the invocations.
			// This is so individual decisions can have unique events when the buttons are used.
			var selectionTrigger = button.GetComponent<SelectionTrigger>();
			if (selectionTrigger != null) {
				selectionTrigger.onSelect.AddListener(decision.Events.OnSelect.Invoke);
				selectionTrigger.onDeselect.AddListener(decision.Events.OnDeselect.Invoke);
			}

			// Set the prompt text of the button, if present
			var buttonText = button.GetComponentInChildren<Text>();
			if (buttonText) buttonText.text = decision.OptionText;
			decisionButtons[decision.OptionText] = button;
		}

		var selector = GetComponent<SelectOnEnable>();
		if (selector != null) selector.TargetSelectable = firstButton;

		currentPlayerIndex = 0;
	}

	/// <summary>
	/// Handles the logic for a player selecting a branch.
	/// </summary>
	/// <param name="decision">The decision data the player that should be tied to the player.</param>
	private void Select(DecisionControlData decision) {
		fork.PickBranch(CurrentPlayer, decision.Data, combineChoices);
		onDecisionMade.Invoke();
	}

	/// <summary>
	/// Prepares the menu for the next player to make a selection. If there is none, the panel will be ended.
	/// </summary>
	private void Advance() {
		currentPlayerIndex++;
		if (currentPlayerIndex < Panel.Board.OwningPlayers.Count) Panel.Menu.Cycle();
		else EndControl();
	}

	/// <summary>Deactivates any decisions that the player has not met the conditions for.</summary>
	private void ConditionPassDecisions() {
		var parser = new ConditionParser(Session.BundleGameData(new List<int> { CurrentPlayer }));

		foreach (var decisionData in decisions) {
			var button = decisionButtons[decisionData.OptionText];
			button.gameObject.SetActive(parser.Evaluate(decisionData.Condition));
		}
	}

	/// <summary>
	/// Refreshes the display of the control if another player needs to choose, 
	/// else calls the Finalize method to end the panel.
	/// </summary>
	public void Refresh() {
		if (currentPlayerIndex >= Panel.Board.OwningPlayers.Count) return;

		var player = CurrentPlayer;
		var character = CharacterManager.GetCharacter(player);
		playerPrompt.text = string.Format(playerPromptFormat, character.Data.Firstname);
		playerPrompt.color = character.Data.Color;
		InputManager.SetActivePlayer(player);
		ConditionPassDecisions();
	}

	/// <summary>Fires off all player choice events then fires the onAllDecisionsMade event.</summary>
	private void EndControl() {
		fork.SendOff();
		onAllDecisionsMade.Invoke();
		Panel.End();
	}
}

[Serializable]
public class DecisionControlData {
	/// <summary>OptionText displays on the button and should tell the player what they are doing.</summary>
	public string OptionText = "";
	/// <summary>An optional condition for determining if a player can choose this option.</summary>
	public string Condition = "";
	/// <summary>Branch data.</summary>
	public Branch Data = new Branch();
	/// <summary>Struct to hold event references so a collapsable region in the inspector.</summary>
	public EventContainer Events = new EventContainer {
		OnSelect = new UnityEvent(),
		OnDeselect = new UnityEvent()
	};

	[Serializable]
	public struct EventContainer {
		/// <summary>Event to fire when button is selected.</summary>
		public UnityEvent OnSelect;
		/// <summary>Event to fire when button is no longer selected.</summary>
		public UnityEvent OnDeselect;
	}
}