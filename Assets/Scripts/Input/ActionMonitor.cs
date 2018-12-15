using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionMonitor : InputMonitor {
	[SerializeField]
	private List<Trigger> triggers = new List<Trigger>();

	public override void CheckForInput() {
		foreach (var trigger in triggers)
			foreach (var button in trigger.Buttons)
				if (InputManager.GetButtonDownFromActive(button))
					trigger.OnButton.Invoke(InputManager.ActivePlayer, button);
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