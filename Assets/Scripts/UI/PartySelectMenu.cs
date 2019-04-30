using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Main GameObject responsible for directing the flow of the Character Selection process.
/// </summary>
[RequireComponent(typeof(MenuGroup))]
public class PartySelectMenu : MonoBehaviour {
	[SerializeField]
	private MenuGroup menuGroup;

	[SerializeField]
	private CharacterCreationMenu creationMenuPrefab;

	[SerializeField]
	private UnityEvent OnAllCharactersSelected;

	private readonly List<CharacterSelectionMenu> selectionMenus = new List<CharacterSelectionMenu>();

	private void OnValidate() {
		if (menuGroup == null) menuGroup = GetComponent<MenuGroup>();
	}

	public void Populate() {
		if (InputManager.PlayerCount <= 0) return;

		selectionMenus.Clear();
		CharacterManager.Clear();

		// Instantiate menus
		foreach (var player in InputManager.AssignedPlayers) {
			var selectionMenu = Instantiate(creationMenuPrefab, transform);
			selectionMenu.name = "CharacterCreationMenu" + player;
			selectionMenu.Initialize(player);

			selectionMenu.Menu.OnOpened.AddListener(() => {
				InputManager.SetActivePlayer(player);
			});

			selectionMenu.gameObject.SetActive(false);
			selectionMenus.Add(selectionMenu);
		}

		// Link menu navigation
		for (var i = 0; i < selectionMenus.Count - 1; i++) {
			var nextMenu = selectionMenus[i + 1].Menu;
			selectionMenus[i].DoneButton.onClick.AddListener(() => {
				menuGroup.ChangeMenu(nextMenu);
			});
		}

		// Set last menu to trigger Finish method
		selectionMenus[selectionMenus.Count - 1].DoneButton.onClick.AddListener(Finish);
		// Set first menu as active
		menuGroup.RefreshMenuList();
		menuGroup.ChangeMenu(selectionMenus[0].Menu);
	}

	public void Finish() {
		foreach (var menu in selectionMenus) {
			if (menu.CharacterData == null)
				Debug.LogWarningFormat("Party Select Menu is finalizing, but a player {0} has no character data.", menu.Player);
			else CharacterManager.SpawnPlayerCharacter(menu.Player, menu.CharacterData);
		}
		OnAllCharactersSelected.Invoke();
	}

	public void Clear() {
		foreach (Transform child in transform) Destroy(child.gameObject);
	}
}
