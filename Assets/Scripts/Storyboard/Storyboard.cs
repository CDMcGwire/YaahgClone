using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Menu))]
[RequireComponent(typeof(MenuGroup))]
[RequireComponent(typeof(Path))]
public class Storyboard : MonoBehaviour {
	public UnityEvent OnBoardFinished = new UnityEvent();

	[SerializeField]
	private Menu menu;

	[SerializeField]
	private MenuGroup menuGroup;

	[SerializeField]
	private Path path;

	[SerializeField]
	private List<int> owningPlayers = new List<int>();
	public List<int> OwningPlayers { get { return owningPlayers; } set { owningPlayers = value; } }
	public List<Character> OwningCharacters { get { return CharacterManager.GetPlayerCharacters(OwningPlayers); } }

	[SerializeField]
	private List<string> traits = new List<string>();
	public List<string> Traits { get { return traits; } }

	public void AddTrait(string trait) {
		if (Traits.Contains(trait)) return;
		Traits.Add(trait);
	}

	public bool HasTrait(string trait) {
		return Traits.Contains(trait);
	}

	public GameData GameData { get; private set; }

	private void OnValidate() {
		if (menu == null) menu = GetComponent<Menu>();
		if (menuGroup == null) menuGroup = GetComponent<MenuGroup>();
		if (path == null) path = GetComponent<ConditionalPath>();
	}

	public void Initialize() {
		menu.OnClosed.AddListener(OnBoardFinished.Invoke);
		
		GameData = new GameData(OwningCharacters, Traits, Session.Traits);
		foreach (var character in OwningCharacters) character.OnDeath.AddListener(HandleCharacterDeath);
		SetupNextPanel(path.GetNextPanel(GameData));
	}

	private void SetupNextPanel(Panel panel) {
		if (panel == null) {
			menu.Close();
			return;
		}
		panel.Initialize(this);
		panel.OnComplete.AddListener(nextPanel => SetupNextPanel(nextPanel));
		menuGroup.ChangeMenu(panel.Menu);
	}

	private void HandleCharacterDeath(Character character, List<CharacterStat> limits) {
		foreach (var stat in limits) AddTrait(DeathTraits.From(stat));
	}
}
