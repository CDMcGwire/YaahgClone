using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class represents a self-contained, playable section of gameplay.
/// </summary>
[RequireComponent(typeof(Menu))]
public abstract class Storyboard : MonoBehaviour {
	public UnityEvent OnBoardFinished = new UnityEvent();

	[Tooltip("The list of players that this storyboard pertains to.")]
	[SerializeField]
	private List<int> owningPlayers = new List<int>();
	public List<int> OwningPlayers { get => owningPlayers; set => owningPlayers = value; }

	/// <summary>Shorthand property to retrieve the Characters of the owning players.</summary>
	public List<Character> OwningCharacters => CharacterManager.GetPlayerCharacters(OwningPlayers);

	[Tooltip("Remove players from the list of owners if they die.")]
	[SerializeField]
	private bool removeDeadPlayers = true;
	public bool RemoveDeadPlayers { get => removeDeadPlayers; set => removeDeadPlayers = value; }

	[Space(12)]

	[Tooltip("Reference to this storyboard's menu component.")]
	[SerializeField]
	private Menu menu;
	public Menu Menu => menu;

	[Tooltip("Menu Group containing all of the panels in this storyboard. (optional)")]
	[SerializeField]
	private OwnerInfoPanel ownerInfoMenu;
	public OwnerInfoPanel OwnerInfoMenu => ownerInfoMenu;

	/// <summary>
	/// The string value to use to identify what encounter this storyboard belongs to.
	/// </summary>
	public abstract string ID { get; }

	/// <summary>
	/// Indicates if this storyboard should be treated as unique by other systems.
	/// </summary>
	public abstract bool Unique { get; }

	/// <summary>
	/// A package containing references to the storyboard's owning characters and traits, 
	/// as well as the current session traits.
	/// </summary>
	public GameData GameData { get; private set; }

	/// <summary>
	/// Retrieves the next panel by whatever logic the child class implements.
	/// Should return null if no more panels are available.
	/// </summary>
	protected abstract Panel NextPanel();

	/// <summary>
	/// Get all uique panel components used by the Storyboard. Implemented by the child
	/// class, as they may manage their panels differently.
	/// </summary>
	protected abstract List<Panel> Panels { get; }

	/// <summary>Check for whether or not first time setup has run on the storyboard.</summary>
	private bool alreadyRun = false;

	protected void OnValidate() {
		if (menu == null) menu = GetComponent<Menu>();
	}
	
	/// <summary>Cleanup on disable.</summary>
	private void OnDisable() {
		foreach (var character in OwningCharacters) character.OnDeath.RemoveListener(HandleCharacterDeath);
	}

	/// <summary>Play the storyboard.</summary>
	public void Play() {
		if (!alreadyRun) {
			Menu.OnClosed.AddListener(OnBoardFinished.Invoke);
			alreadyRun = true;
		}
		// If no characters own the storyboard, it will default to all living players.
		if (OwningPlayers.Count < 1) OwningPlayers = CharacterManager.LivingPlayers;

		GameData = new GameData(OwningCharacters, Session.EncounterTraits, Session.Traits);
		foreach (var character in OwningCharacters) character.OnDeath.AddListener(HandleCharacterDeath);

		// Setup panel flow
		foreach (var panel in Panels) {
			panel.OnEnd.RemoveAllListeners();
			panel.OnEnd.AddListener(StartNextPanel);
			panel.gameObject.SetActive(false);
		}
		// Perform any additional setup that subclasses may require
		AdditionalSetup();
		// Begin Playback
		Menu.Open();
		StartNextPanel();
	}

	/// <summary>Trigger the end procedure for the storyboard.</summary>
	public void End() {
		foreach (var character in OwningCharacters) character.OnDeath.RemoveListener(HandleCharacterDeath);
		Menu.Close();
	}

	/// <summary>Override to give additional logic on first time setup.</summary>
	protected virtual void AdditionalSetup() { }

	/// <summary>If the next panel is valid, initialize and enable it, otherwise finish the board.</summary>
	protected void StartNextPanel() => _ = StartCoroutine(DelayedPanelStart());

	/// <summary>
	/// PanelStart logic is handled as a coroutine to give the last panel a chance to finish all of its
	/// Monobehaviour methods for the frame.
	/// </summary>
	/// <returns>A single frame Enumerator.</returns>
	private IEnumerator DelayedPanelStart() {
		yield return null;
		var panel = NextPanel();
		// No More panels. Close the storyboard.
		if (panel == null) {
			End();
			yield break;
		}
		panel.Setup(this);
		panel.Play();
	}

	/// <summary>Perform any actions necessary when a player dies on this storyboard, like setting traits.</summary>
	/// <param name="character">Character that died.</param>
	/// <param name="limits">Limits the character reached when they died.</param>
	private void HandleCharacterDeath(Character character, List<CharacterStat> limits) {
		foreach (var stat in limits) Session.AddEncounterTrait(DeathTraits.From(stat));
		if (removeDeadPlayers) _ = owningPlayers.Remove(character.PlayerNumber);
		character.OnDeath.RemoveListener(HandleCharacterDeath);
	}
}
