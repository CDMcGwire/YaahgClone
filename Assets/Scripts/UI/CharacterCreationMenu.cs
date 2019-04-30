using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu used to create a new character.
/// </summary>
[RequireComponent(typeof(Menu))]
public class CharacterCreationMenu : CharacterSelectionMenu {
	[SerializeField]
	private Text menuPrompt;

	[SerializeField]
	private string menuPromptTemplate = "Player {0}, Create your Character";

	[SerializeField]
	private InputField nameInput;
	private InputField NameInput => nameInput;

	[SerializeField]
	private ToggleGroup titleSelector;
	private ToggleGroup TitleSelector => titleSelector;

	[SerializeField]
	private ToggleGroup colorSelector;
	private ToggleGroup ColorSelector => colorSelector;

	[Space(12)]
	[Tooltip("Reference to TitleGenerator scriptable object.")]
	[SerializeField]
	private TitleGenerator titleGenerator;

	private string characterName;
	private CharacterData startingData;
	private Color characterColor = Color.black;

	public override void Initialize(int player) {
		Debug.Assert(menuPrompt, "Menu Prompt not configured for " + name);
		Debug.Assert(nameInput, "Name Input not configured for " + name);
		Debug.Assert(titleSelector, "Title Selector not configured for " + name);
		Debug.Assert(colorSelector, "Color Selector not configured for " + name);

		Player = player;
		menuPrompt.text = string.Format(menuPromptTemplate, Player + 1);

		// Setup name input
		nameInput.onEndEdit.AddListener(input => {
			characterName = input != null ? input.Trim() : "";
			ValidateInput();
		});

		// Setup title selector
		foreach (var toggle in titleSelector.GetComponentsInChildren<Toggle>()) {
			var textHolder = toggle.GetComponentInChildren<Text>();
			var titleData = titleGenerator.Next;
			textHolder.text = "The " + titleData.Title;

			toggle.onValueChanged.AddListener(isOn => {
				if (isOn) {
					startingData = titleData;
					ValidateInput();
				}
			});
		}

		// Setup color select input
		foreach (var toggle in colorSelector.GetComponentsInChildren<Toggle>()) {
			var colorImage = toggle.GetComponent<Image>();

			toggle.onValueChanged.AddListener(isOn => {
				if (isOn) {
					characterColor = colorImage.color;
					ValidateInput();
				}
			});
		}

		// Setup Done button
		DoneButton.onClick.AddListener(FinalizeCharacter);

		ValidateInput();
	}

	/// <summary>Activate the "Done" button only if every control has a valid input.</summary>
	private void ValidateInput() {
		if (string.IsNullOrWhiteSpace(characterName)
			|| startingData == null
			|| characterColor == Color.black) {
			DoneButton.interactable = false;
		}
		else DoneButton.interactable = true;
	}

	/// <summary>Populates a character data object from the chosen inputs and spawns the characters.</summary>
	private void FinalizeCharacter() {
		CharacterData = new CharacterData(startingData) {
			Name = characterName,
			Color = characterColor,
			Alive = true
		};
	}
}
