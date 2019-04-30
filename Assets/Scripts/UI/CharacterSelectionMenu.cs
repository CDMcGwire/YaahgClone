using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Abstract class representing a menu that can be used for slecting a player character.
/// </summary>
public abstract class CharacterSelectionMenu : MonoBehaviour {
	[Tooltip("Reference to the Menu component for the game object.")]
	[SerializeField]
	private Menu menu;
	public Menu Menu => menu;

	[Tooltip("Reference to the Button to use as the done button.")]
	[SerializeField]
	private Button doneButton;
	public Button DoneButton => doneButton;

	/// <summary>Property to store the owning player.</summary>
	public int Player { get; protected set; }

	/// <summary>Used to retrieve the menu's character data.</summary>
	public CharacterData CharacterData { get; protected set; }

	/// <summary>Use to prepare the menu for use.</summary>
	/// <param name="player">The player who should own the menu.</param>
	public abstract void Initialize(int player);
}
