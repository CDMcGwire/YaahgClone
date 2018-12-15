using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MenuGroup))]
public class CharacterSelectMenu : MonoBehaviour {
	[SerializeField]
	private MenuGroup menuGroup;

	[SerializeField]
	private CharacterCreationMenu creationMenuPrefab;

	[SerializeField]
	private UnityEvent OnAllCharactersSelected;

	private void OnValidate() {
		if (menuGroup == null) menuGroup = GetComponent<MenuGroup>();
	}

	public void Populate() {
		if (InputManager.PlayerCount <= 0) return;

		var creationMenus = new List<CharacterCreationMenu>();

		// Instantiate menus
		foreach (var player in InputManager.AssignedPlayers) {
			var creationMenu = Instantiate(creationMenuPrefab, transform);
			creationMenu.Player = player;
			creationMenu.name = "CharacterCreationMenu" + player;

			creationMenu.Menu.OnOpened.AddListener(() => {
				InputManager.SetActivePlayer(player);
			});

			creationMenu.gameObject.SetActive(false);
			creationMenus.Add(creationMenu);
		}

		// Link menu navigation
		for (var i = 0; i < creationMenus.Count - 1; i++) {
			var nextMenu = creationMenus[i + 1].Menu;
			creationMenus[i].DoneButton.onClick.AddListener(() => {
				menuGroup.ChangeMenu(nextMenu);
			});
		}

		// Set last menu to trigger Finish method
		creationMenus[creationMenus.Count - 1].DoneButton.onClick.AddListener(Finish);
		// Set first menu as active
		menuGroup.RefreshMenuList();
		menuGroup.ChangeMenu(creationMenus[0].Menu);
	}

	public void Finish() {
		OnAllCharactersSelected.Invoke();
	}

	public void Clear() {
		foreach (Transform child in transform) Destroy(child.gameObject);
	}
}
