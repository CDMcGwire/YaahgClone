using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerJoinMonitor : InputMonitor {
	[SerializeField]
	private List<string> joinButtons = new List<string>{ "Submit", "Menu" };
	[SerializeField]
	private List<string> cancelButtons = new List<string>{ "Back" };

	[SerializeField]
	public UnityEvent OnTriggered = new UnityEvent();

	[SerializeField]
	private bool seekingInput = false;
	public bool SeekingInput { get => seekingInput; set => seekingInput = value; }

	private HashSet<string> controllers;
	private Dictionary<string, List<string>> controllerButtons;
	
	private void Start() {
		controllers = new HashSet<string>(InputManager.ControllerCodeNames);

		controllerButtons = new Dictionary<string, List<string>>();

		foreach (var code in InputManager.ControllerCodeNames) {
			var buttonStrings = new List<string>(joinButtons.Count);
			foreach (var button in joinButtons) buttonStrings.Add(code + "_" + button);
			controllerButtons[code] = buttonStrings;
		}
	}

	public override void CheckForInput() {
		if (!seekingInput) return;

		foreach (var button in cancelButtons) {
			if (InputManager.GetButtonDownFromActive(button)) Close();
		}

		foreach (var controller in controllers) {
			if (CheckForControllerInput(controller)) {
				InputManager.AddPlayer(controller);
				Close();
			}
		}
	}

	private void Close() {
		OnTriggered.Invoke();
		seekingInput = false;
	}

	private bool CheckForControllerInput(string controller) {
		foreach (var button in controllerButtons[controller]) {
			if (Input.GetButtonDown(button)) return true;
		}
		return false;
	}
}