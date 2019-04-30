using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Maps player inputs to Parameterized Events, then monitors for those inputs to activate.
/// </summary>
public class ActionMonitor : InputMonitor {
	[Tooltip("The set of events that can fire and their corresponding buttons.")]
	[SerializeField]
	private List<Trigger> triggers = new List<Trigger>();

	[Tooltip("An optional list of players to check for. Otherwise defaults to the active player.")]
	[SerializeField]
	private List<int> players = new List<int>();
	public List<int> Players { get => players; set => players = value; }

	[Tooltip("Should this event fire once per triggering player or only on the first player?")]
	[SerializeField]
	private bool firstOnly = true;
	public bool FirstOnly { get => firstOnly; set => firstOnly = value; }

	public override void CheckForInput() {
		foreach (var trigger in triggers)
			foreach (var button in trigger.Buttons)
				CheckForTrigger(trigger.OnButton, button);
	}

	/// <summary>Fire the trigger events if the mapped input was received from the watched players.</summary>
	/// <param name="onButton">The event to invoke.</param>
	/// <param name="button">The triggering button to check.</param>
	private void CheckForTrigger(ButtonEvent onButton, string button) {
		if (players == null || players.Count < 1) {
			if (InputManager.GetButtonDownFromActive(button))
				onButton.Invoke(InputManager.ActivePlayer, button);
		}
		else {
			foreach (var player in players) {
				if (InputManager.GetPlayerButtonDown(player, button)) {
					onButton.Invoke(player, button);
					if (firstOnly) return;
				}
			}
		}
	}
}

[Serializable]
public class ButtonEvent : UnityEvent<int, string> { }

[Serializable]
public class Trigger {
	public string[] Buttons = { "Submit" };
	public ButtonEvent OnButton = new ButtonEvent();
}

[Serializable]
public enum TargetPlayerType {
	Specific,
	Active,
	Any
}