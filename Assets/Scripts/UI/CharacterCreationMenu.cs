using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Menu))]
public class CharacterCreationMenu : MonoBehaviour {
	[SerializeField]
	private Menu menu;
	public Menu Menu { get { return menu; } }

	[SerializeField]
	private int player = -1;
	public int Player { get { return player; } set { player = value; menuPrompt.text = string.Format(menuPromptTemplate, value + 1); } }

	[SerializeField]
	private Text menuPrompt;

	[SerializeField]
	private string menuPromptTemplate = "Player {0}, Create your Character";

	[SerializeField]
	private InputField nameInput;
	private InputField NameInput { get { return nameInput; } }

	[SerializeField]
	private ToggleGroup titleSelector;
	private ToggleGroup TitleSelector { get { return titleSelector; } }

	[SerializeField]
	private ToggleGroup colorSelector;
	private ToggleGroup ColorSelector { get { return colorSelector; } }

	[SerializeField]
	private Button doneButton;
	public Button DoneButton { get { return doneButton; } }

	[Space(12)]
	[Tooltip("Reference to TitleGenerator scriptable object.")]
	[SerializeField]
	private TitleGenerator titleGenerator;

	private CharacterData characterData = new CharacterData();
	private bool wasColorSelected = false;

	private void OnValidate() {
		if (!menu) menu = GetComponent<Menu>();
	}

	private void Awake() {
		Debug.Assert(menuPrompt, "Menu Prompt not configured for " + name);
		Debug.Assert(nameInput, "Name Input not configured for " + name);
		Debug.Assert(titleSelector, "Title Selector not configured for " + name);
		Debug.Assert(colorSelector, "Color Selector not configured for " + name);
		Debug.Assert(doneButton, "Done Button not configured for " + name);

		characterData.Alive = true;
		SetDefaultStats(characterData);

		// Setup name input
		nameInput.onEndEdit.AddListener(input => {
			characterData.Name = input != null ? input.Trim() : "";
			ValidateInput();
		});

		// Setup title selector
		foreach (var toggle in titleSelector.GetComponentsInChildren<Toggle>()) {
			var textHolder = toggle.GetComponentInChildren<Text>();
			var title = titleGenerator.Next;
			textHolder.text = "The " + title;

			toggle.onValueChanged.AddListener(isOn => {
				if (isOn) {
					characterData.Title = title;
					ValidateInput();
				}
			});
		}

		// Setup color select input
		foreach (var toggle in colorSelector.GetComponentsInChildren<Toggle>()) {
			var colorImage = toggle.GetComponent<Image>();

			toggle.onValueChanged.AddListener(isOn => {
				if (isOn) {
					characterData.Color = colorImage.color;
					wasColorSelected = true;
					ValidateInput();
				}
			});
		}

		// Setup Done button
		doneButton.onClick.AddListener(() => CharacterManager.SpawnPlayerCharacter(player, characterData));

		ValidateInput();
	}

	private void ValidateInput() {
		if (characterData.Name == null || characterData.Name.Length <= 0
			|| characterData.Title == null || characterData.Title.Length <= 0
			|| !wasColorSelected) {
			doneButton.interactable = false;
		}
		else doneButton.interactable = true;
	}

	private static void SetDefaultStats(CharacterData data) {
		data.Strength = 3;
		data.Toughness = 3;
		data.Dexterity = 3;
		data.Perception = 3;
		data.Knowledge = 3;
		data.Magic = 3;
		data.Charisma = 3;
		data.Willpower = 3;
		data.Lethality = 3;
		data.Money = 300;

		data.PhysicalThreshold = -3;
		data.MentalThreshold = -3;
		data.DebtThreshold = -3;
	}
}
