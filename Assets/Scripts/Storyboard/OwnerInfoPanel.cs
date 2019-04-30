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
	public Menu Menu => menu;

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

	/// <summary>
	/// A stack of dynamically spawned name displays which should be cleared when repopulated.
	/// </summary>
	private Stack<GameObject> dynamicDisplays = new Stack<GameObject>();

	/// <summary>Setup all relevant display objects using the data passed in.</summary>
	/// <param name="gameData">Container for relevant game data: characters, traits, etc.</param>
	public void Initialize(List<int> owningPlayers) {
		var characters = CharacterManager.GetPlayerCharacters(owningPlayers);

		// Cleanup old displays
		while (dynamicDisplays.Count > 0) Destroy(dynamicDisplays.Pop());

		if (CharacterManager.IsParty(characters)) {
			PopulateNameDisplay(nameDisplayObject, partyDisplayText, partyDisplayAccent);
			nameDisplayObject.SetActive(true);
		}
		else if (characters.Count == 1) {
			var characterData = characters[0].Data;
			var displayText = characterData.Name + " the " + characterData.Title;
			PopulateNameDisplay(nameDisplayObject, displayText, characterData.Color);
			nameDisplayObject.SetActive(true);
		}
		else {
			nameDisplayObject.SetActive(false);
			foreach (var character in characters) {
				var display = Instantiate(nameDisplayObject, nameDisplayParent);
				PopulateNameDisplay(display, character.Data.Firstname, character.Data.Color);
				display.SetActive(true);
				dynamicDisplays.Push(display);
			}
		}
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
