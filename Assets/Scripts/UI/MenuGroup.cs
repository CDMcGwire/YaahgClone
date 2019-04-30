using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Groups menu components directly parented under the game object this component is attached to.
/// It acts as an interface to manage swapping between active menus and maintaining a simple history.
/// 
/// Managed menus are stored by name, where child elements with the same name overwrite each
/// other in the mapping. Thus all children should be given unique names.
/// </summary>
public class MenuGroup : MonoBehaviour {
	[SerializeField]
	private Menu initialMenu;

	private Dictionary<string, Menu> availableMenus = new Dictionary<string, Menu>(0);

	/// <summary>The currently active menu.</summary>
	private Menu activeMenu;

	private Stack<Menu> history = new Stack<Menu>();

	/// <summary>
	/// Attempts to trigger a menu transition from whatever is currently active (if anything) to
	/// the given menu game object.
	/// </summary>
	/// <param name="nextMenu">The target menu to transition to. If null, the active menu will be closed.</param>
	/// <param name="cycleIfCurrent">
	/// If the given menu is already the active menu, should it be cycled? 
	/// Otherwise the command will be ignored.
	/// </param>
	public void ChangeMenu(Menu nextMenu, bool cycleIfCurrent = false) {
		// Do nothing if the previous menu is closing or the specified menu either doesn't exist or is no longer valid
		if ((activeMenu != null && activeMenu.CurrentState == Menu.State.Closing) 
			|| !availableMenus.ContainsKey(nextMenu.name) 
			|| availableMenus[nextMenu.name] != nextMenu)
			return;
		// If the target is null, close everything.
		else if (nextMenu == null) {
			activeMenu.Close();
			activeMenu = null;
			return;
		}
		// If the target is the current menu check if it should cycle, else do nothing
		else if (nextMenu == activeMenu) {
			if (cycleIfCurrent) nextMenu.Cycle();
			return;
		}

		var lastMenu = activeMenu;
		activeMenu = availableMenus[nextMenu.name];
		// Set next immediately and temporarily store last menu for instances where Menu does not close immediately
		if (lastMenu is null) activeMenu.Open();
		else {
			// Close last menu and set current to open when complete
			void OnLastMenuClosed() {
				activeMenu.Open();
				lastMenu.OnClosed.RemoveListener(OnLastMenuClosed);
			}
			lastMenu.OnClosed.AddListener(OnLastMenuClosed);
			lastMenu.Close();
			// Add the old menu to the history stack
			history.Push(lastMenu);
		}
	}

	/// <summary>
	/// Switch the menu back to the last valid one on the history stack, if any.
	/// </summary>
	public void Return() {
		Menu lastMenu = null;
		// Find the first non-null menu on the stack; Prevents errors when a menu is destroyed
		while (!lastMenu && history.Count > 0) {
			lastMenu = history.Pop();
		}

		// Close the current menu and ready the next to open
		if (lastMenu) {
			var currentMenu = activeMenu;
			activeMenu = lastMenu;
			currentMenu.Close();
		}
	}

	/// <summary>Closes the active menu without switching to a new one.</summary>
	public void CloseAll() => ChangeMenu(null);

	/// <summary>
	/// Invalidate any existing record of managed menus and rebuild it from the current
	/// set of child elements.
	/// </summary>
	public void RefreshMenuList() {
		// Clear list of invalid menus
		var deadKeys = new List<string>();
		foreach (var set in availableMenus) deadKeys.Add(set.Key);
		foreach (var key in deadKeys) _ = availableMenus.Remove(key);

		foreach (Transform child in transform) {
			if (!availableMenus.ContainsKey(child.name)) {
				var menu = child.GetComponent<Menu>();
				if (menu) availableMenus[menu.name] = menu;
			}
		}
	}

	public void OnEnable() {
		RefreshMenuList();
		foreach (var menu in availableMenus.Values) menu.gameObject.SetActive(false);

		if (initialMenu && !activeMenu) {
			activeMenu = initialMenu;
			activeMenu.Open();
		}
	}
}
