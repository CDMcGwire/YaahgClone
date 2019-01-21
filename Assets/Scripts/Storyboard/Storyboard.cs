using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class represents a self-contained, playable section of gameplay.
/// </summary>
[RequireComponent(typeof(Menu))]
[RequireComponent(typeof(MenuGroup))]
[RequireComponent(typeof(Path))]
public class Storyboard : MonoBehaviour {
	public UnityEvent OnBoardFinished = new UnityEvent();

	/// <summary>If true, the storyboard should not be repeated in a session.</summary>
	[SerializeField]
	private bool unique = true;
	public bool Unique { get { return unique; } }

	/// <summary>Reference to this storyboard's menu component.</summary>
	[SerializeField]
	private Menu menu;
	public Menu Menu { get { return menu; } }

	/// <summary>Menu Group containing all of the panels in this storyboard.</summary>
	[SerializeField]
	private MenuGroup panelGroup;

	/// <summary>Initial path node that determines the start of the storyboard.</summary>
	[SerializeField]
	private Path initialPath;

	/// <summary>The list of players that this storyboard pertains to.</summary>
	[SerializeField]
	private List<int> owningPlayers = new List<int>();
	public List<int> OwningPlayers { get { return owningPlayers; } set { owningPlayers = value; } }

	/// <summary>Shorthand property to retrieve the Characters of the owning players.</summary>
	public List<Character> OwningCharacters { get { return CharacterManager.GetPlayerCharacters(OwningPlayers); } }

	/// <summary>Storyboard persistent data for use in evaluating paths.</summary>
	[SerializeField]
	private List<string> traits = new List<string>();
	public List<string> Traits { get { return traits; } }

	[Space(12)]

	/// <summary>Menu Group containing all of the panels in this storyboard. (optional)</summary>
	[SerializeField]
	private OwnerInfoPanel ownerInfoMenu;

	/// <summary>Store some information on the storyboard's Trait list.</summary>
	/// <param name="trait">The name of the trait to store.</param>
	public void AddTrait(string trait) {
		if (Traits.Contains(trait)) return;
		Traits.Add(trait);
	}

	/// <summary>Check if a trait is present on this storyboard.</summary>
	/// <param name="trait">The trait to look for.</param>
	/// <returns>True if found.</returns>
	public bool HasTrait(string trait) {
		return Traits.Contains(trait);
	}

	/// <summary>
	/// A package containing references to the storyboard's owning characters and traits, 
	/// as well as the current session traits.
	/// </summary>
	public GameData GameData { get; private set; }

	private void OnValidate() {
		if (menu == null) menu = GetComponent<Menu>();
		if (panelGroup == null) panelGroup = GetComponent<MenuGroup>();
		if (initialPath == null) initialPath = GetComponent<ConditionalPath>();
	}

	/// <summary>Prep the storyboard for playback.</summary>
	public void Initialize() {
		menu.OnClosed.AddListener(OnBoardFinished.Invoke);

		// If no characters own the storyboard, it will default to all living players.
		if (OwningPlayers.Count < 1) OwningPlayers = CharacterManager.LivingPlayers;

		GameData = new GameData(OwningCharacters, Traits, Session.Traits);
		foreach (var character in OwningCharacters) character.OnDeath.AddListener(HandleCharacterDeath);

		if (ownerInfoMenu) ownerInfoMenu.Initialize(GameData);
		SetupNextPanel(initialPath.GetNextPanel(GameData));
	}

	/// <summary>If the next panel is valid, initialize and enable it, otherwise finish the board.</summary>
	/// <param name="panel">The next panel to display or null to close the storyboard.</param>
	private void SetupNextPanel(Panel panel) {
		if (panel == null) {
			menu.Close();
			return;
		}
		panel.Initialize(this);
		panel.OnComplete.AddListener(nextPanel => SetupNextPanel(nextPanel));
		panelGroup.ChangeMenu(panel.Menu);

		if (ownerInfoMenu) {
			if (panel.ShowOwnerInfo) ownerInfoMenu.Menu.Open();
			else ownerInfoMenu.Menu.Close();
		}
	}

	/// <summary>Perform any actions necessary when a player dies on this storyboard, like setting traits.</summary>
	/// <param name="character">Character that died.</param>
	/// <param name="limits">Limits the character reached when they died.</param>
	private void HandleCharacterDeath(Character character, List<CharacterStat> limits) {
		foreach (var stat in limits) AddTrait(DeathTraits.From(stat));
	}
}
