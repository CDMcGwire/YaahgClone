using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManagementMenu : MonoBehaviour {
	[SerializeField]
	private Button activePlayerButtonPrefab;

	[SerializeField]
	private Transform activePlayerButtonParent;

	private Dictionary<int, Button> activePlayerButtons = new Dictionary<int, Button>();

	private void OnEnable() {
		if (!InputManager.Instance) return;
		InputManager.OnPlayerJoin.AddListener(AddPlayerButton);
		InputManager.OnPlayerLeft.AddListener(RemovePlayerButton);

		if (!activePlayerButtonParent) activePlayerButtonParent = transform;
		
		foreach (var player in InputManager.AssignedPlayers)
			AddPlayerButton(player, InputManager.GetPlayerControllerCode(player));
	}

	private void OnDisable() {
		if (!InputManager.Instance) return;
		InputManager.OnPlayerJoin.RemoveListener(AddPlayerButton);
		InputManager.OnPlayerLeft.RemoveListener(RemovePlayerButton);

		foreach (var button in activePlayerButtons.Values) if (button) Destroy(button.gameObject);
		activePlayerButtons.Clear();
	}

	private void AddPlayerButton(int player, string controller) {
		var activePlayerButton = Instantiate(activePlayerButtonPrefab, activePlayerButtonParent);

		var buttonText = "Player " + (player + 1) + " on " + InputManager.GetControllerName(controller);
		activePlayerButton.GetComponentInChildren<Text>().text = buttonText;

		activePlayerButton.onClick.AddListener(() => {
			InputManager.RemovePlayer(player);
			Destroy(activePlayerButton.gameObject);
		});

		activePlayerButtons[player] = activePlayerButton;
	}

	private void RemovePlayerButton(int player, string controller) {
		if (activePlayerButtons.ContainsKey(player)) {
			Destroy(activePlayerButtons[player]);
			activePlayerButtons[player] = null;
		}
	}
}
