using System.Collections.Generic;
using UnityEngine;

public class MenuGroup : MonoBehaviour {
	[SerializeField]
	private Menu initialMenu;

	private Dictionary<string, Menu> availableMenus = new Dictionary<string, Menu>(0);

	private Menu activeMenu;

	private Stack<Menu> menuStack = new Stack<Menu>();

	public void ChangeMenu(Menu nextMenu) {
		// Do nothing if the specified menu doesn't exist, is no longer valid, or menu is already active
		if (!nextMenu
			|| !availableMenus.ContainsKey(nextMenu.name)
			|| availableMenus[nextMenu.name] != nextMenu
			|| activeMenu == nextMenu
			) return;

		// Set next immediately and temporarily store last menu for instances where Menu closes instantly
		var lastMenu = activeMenu;
		activeMenu = availableMenus[nextMenu.name];

		// Add current menu to the history stack
		if (lastMenu) {
			lastMenu.Close();
			menuStack.Push(lastMenu);
		}
		else { // If this is the first menu
			activeMenu.gameObject.SetActive(true);
		}
	}

	public void Return() {
		Menu lastMenu = null;
		// Find the first non-null menu on the stack; Prevents errors when a menu is destroyed
		while (!lastMenu && menuStack.Count > 0) {
			lastMenu = menuStack.Pop();
		}

		// Close the current menu and ready the next to open
		if (lastMenu) {
			var currentMenu = activeMenu;
			activeMenu = lastMenu;
			currentMenu.Close();
		}
	}

	/// <summary>Closes the active menu without switching to a new one.</summary>
	public void CloseAll() {
		ChangeMenu(null);
	}

	public void RefreshMenuList() {
		// Clear list of invalid menus
		var deadKeys = new List<string>();
		foreach (var set in availableMenus) {
			if (set.Value) set.Value.OnClosed.RemoveListener(OnMenuClosed);
			deadKeys.Add(set.Key);
		}
		foreach (var key in deadKeys) availableMenus.Remove(key);

		foreach (Transform child in transform) {
			if (!availableMenus.ContainsKey(child.name)) {
				var menu = child.GetComponent<Menu>();
				if (menu) {
					availableMenus[menu.name] = menu;
					menu.OnClosed.AddListener(OnMenuClosed);
				}
			}
		}
	}

	/// <summary>
	/// Callback method to add to each grouped menu's OnClosed event. Only method responsible for opening menus.
	/// </summary>
	private void OnMenuClosed() {
		activeMenu.gameObject.SetActive(true);
	}

	private void Awake() {
		RefreshMenuList();
		foreach (var menu in availableMenus.Values) menu.gameObject.SetActive(false);

		if (initialMenu && !activeMenu) {
			initialMenu.gameObject.SetActive(true);
			activeMenu = initialMenu;
		}
	}
}
