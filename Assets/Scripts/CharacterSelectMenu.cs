using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectMenu : MonoBehaviour {
	[SerializeField]
	private PlayerJoinMonitor playerJoinMonitor;

	[SerializeField]
	private List<CharacterSelection> characters;

	[SerializeField]
	private CharacterSelector selectorPrefab;

	private void OnValidate() {
		if (playerJoinMonitor == null) playerJoinMonitor = FindObjectOfType<PlayerJoinMonitor>();
		characters = new List<CharacterSelection>(GetComponentsInChildren<CharacterSelection>());
		Debug.Assert(selectorPrefab != null, "No selector prefab found for CharacterSelect menu");
	}

	public void SpawnSelector(int playerNumber, string controllerCode) {
		var selector = Instantiate(selectorPrefab, gameObject.transform);
		selector.PlayerNumber = playerNumber;
		selector.Selections = characters;
		selector.OnSubmit.AddListener(() => {
			selector.CurrentSelection.Select();
			playerJoinMonitor.ReleaseController(controllerCode);
			Destroy(selector);
		});
		selector.OnBack.AddListener(() => {
			playerJoinMonitor.ReleaseController(controllerCode);
			InputManager.RemovePlayer(playerNumber);
			Destroy(selector);
		});
	}
}
