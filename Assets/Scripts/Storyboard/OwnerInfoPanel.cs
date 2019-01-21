using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages references and provides and interface for the storyboard to manage the
/// display of owner information.
/// </summary>
public class OwnerInfoPanel : MonoBehaviour {
	/// <summary>Reference to the controlling menu component.</summary>
	[SerializeField]
	private Menu menu;
	public Menu Menu { get { return menu; } }

	/// <summary>
	/// Reference to a game object with a text field. If all living players are
	/// own the storyboard, it will be enabled on its own with relevant text.
	/// Otherwise it will be copied for each player and character info will show.
	/// </summary>
	[SerializeField]
	private GameObject nameDisplayObject;

	/// <summary>Transform the name display objects should be parented to.</summary>
	[SerializeField]
	private Transform nameDisplayParent;

	[Space(12)]

	/// <summary>Text to display in the name entries when it's the whole party.</summary>
	[SerializeField]
	private string partyDisplayText = "Party";

	/// <summary>Color to set the accent in name entries when it's the whole party.</summary>
	[SerializeField]
	private Color partyDisplayAccent = Color.white;

	/// <summary>Setup all relevant display objects using the data passed in.</summary>
	/// <param name="gameData">Container for relevant game data: characters, traits, etc.</param>
	public void Initialize(GameData gameData) {
		if (IsParty(gameData.Characters)) {
			PopulateNameDisplay(nameDisplayObject, partyDisplayText, partyDisplayAccent);
			nameDisplayObject.SetActive(true);
		}
		else if (gameData.Characters.Count == 1) {
			var characterData = gameData.Characters[0].Data;
			var displayText = characterData.Name + " the " + characterData.Title;
			PopulateNameDisplay(nameDisplayObject, displayText, characterData.Color);
			nameDisplayObject.SetActive(true);
		}
		else {
			nameDisplayObject.SetActive(false);
			foreach (var character in gameData.Characters) {
				var display = Instantiate(nameDisplayObject, nameDisplayParent);
				PopulateNameDisplay(display, character.Data.Firstname, character.Data.Color);
				display.SetActive(true);
			}
		}
	}

	/// <summary>Checks if the list of characters passed contains more than one character and all living characters.</summary>
	/// <param name="owningCharacters">List of characters to display.</param>
	/// <returns>True if the set of living characters contains all characters to display.</returns>
	private bool IsParty(IList<Character> owningCharacters) {
		if (owningCharacters.Count < 2) return false;
		var owners = new HashSet<Character>(owningCharacters);
		foreach (var character in CharacterManager.LivingCharacters)
			if (!owners.Contains(character)) return false;
		return true;
	}

	/// <summary>Attempts to set text and image components of the game objects passed in.</summary>
	/// <param name="display">Game object to use for display.</param>
	/// <param name="text">The 'name' or similar title to display.</param>
	/// <param name="accent">The color to set image components to on the display.</param>
	private void PopulateNameDisplay(GameObject display, string text, Color accent) {
		var textComponent = display.GetComponentInChildren<Text>();
		var imageComponents = display.GetComponentsInChildren<Image>();

		if (textComponent != null) textComponent.text = text; // Space for padding
		foreach (var image in imageComponents) image.color = accent;
	}
}
