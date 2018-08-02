using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ControllerHeldEvent : UnityEvent<string> { }

public class PlayerJoinMonitor : MonoBehaviour {
	public List<string> Buttons = new List<string>{ "Submit", "Back", "Menu" };
	public List<string> Axes = new List<string>{ "Horizontal", "Vertical" };
	public float AxisDeadzone = 0.2f;

	private HashSet<string> heldControllers;
	private HashSet<string> freeControllers;
	private Dictionary<string, List<string>> playerButtons;
	private Dictionary<string, List<string>> playerAxes;

	public void ReleaseController(string controllerCode) {
		if (heldControllers.Remove(controllerCode)) freeControllers.Add(controllerCode);
	}
	
	private void Start() {
		freeControllers = new HashSet<string>(InputManager.ControllerCodeNames);

		playerButtons = new Dictionary<string, List<string>>();
		playerAxes = new Dictionary<string, List<string>>();
		
		foreach (var code in InputManager.ControllerCodeNames) {
			var buttonStrings = new List<string>(Buttons.Count);
			foreach (var button in Buttons) {
				buttonStrings.Add(code + "_" + button);
			}
			playerButtons[code] = buttonStrings;

			var axisStrings = new List<string>(Axes.Count);
			foreach (var axis in Axes) {
				axisStrings.Add(code + "_" + axis);
			}
			playerAxes[code] = axisStrings;
		}

		InputManager.OnPlayerJoin.AddListener((num, code) => Debug.Log("Player " + num + " has joined on controller " + code));
		InputManager.OnPlayerLeft.AddListener((num, code) => Debug.Log("Player " + num + " has left on controller " + code));
	}

	private void Update() {
		var controllerHeld = false;
		foreach (var controller in freeControllers) {
			if (CheckForPlayerInput(controller)) {
				if (InputManager.AddPlayer(controller) > -1) {
					heldControllers.Add(controller);
					controllerHeld = true;
				}
			}
		}
		if (controllerHeld) freeControllers.ExceptWith(heldControllers);
	}

	private bool CheckForPlayerInput(string player) {
		foreach (var button in playerButtons[player]) {
			if (Input.GetButtonDown(button)) return true;
		}
		foreach (var axis in playerAxes[player]) {
			if (Mathf.Abs(Input.GetAxis(axis)) >= AxisDeadzone) return true;
		}
		return false;
	}
}
