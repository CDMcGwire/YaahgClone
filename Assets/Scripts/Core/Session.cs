using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Essentially the game manager for a play session. Holds data that should persist for
/// the entirety and does not belong to a particular character. Lifetime expected to coincide
/// with the Scene it is placed in.
/// </summary>
public class Session : MonoBehaviour {
	/// <summary>Singleton reference.</summary>
	private static Session current;
	public static Session Current {
		get {
			if (current == null) current = FindObjectOfType<Session>();
			return current;
		}
	}

	[Tooltip("A sort of history tracker for a given Session. Used to determine paths.")]
	[SerializeField]
	private List<string> traits = new List<string>();
	public static List<string> Traits => Current?.traits;

	/// <summary>Adds the given trait to the Session if not already present.</summary>
	/// <param name="trait">The trait identifier.</param>
	public static void AddTrait(string trait) {
		if (Current == null) return;
		if (!Traits.Contains(trait)) Traits.Add(trait);
	}

	[Tooltip("Used to track history for the current Encounter. Cleared when it ends.")]
	[SerializeField]
	private List<string> encounterTraits = new List<string>();
	public static List<string> EncounterTraits => Current.encounterTraits;

	/// <summary>Adds the given trait to the current Encounter if not already present.</summary>
	/// <param name="trait">The trait identifier.</param>
	public static void AddEncounterTrait(string trait) {
		if (Current == null) return;
		if (!EncounterTraits.Contains(trait)) EncounterTraits.Add(trait);
	}

	[Tooltip("Trait added on all players dead.")]
	[SerializeField]
	private string partyWipeTrait = "Party Wipe";
	public static string PartyWipeTrait => Current.partyWipeTrait;

	[Space(12)]

	[Tooltip("How long the adventure should last in days.")]
	[SerializeField]
	private int adventureLength = 5;
	public static int AdventureLength { get => Current.adventureLength; set => Current.adventureLength = value; }

	[Tooltip("Event that fires at the end of an Encounter.")]
	[SerializeField]
	private UnityEvent onEncounterEnd = new UnityEvent();
	public static UnityEvent OnEncounterEnd => Current.onEncounterEnd;

	[Tooltip("Event that fires at the end of a game day.")]
	[SerializeField]
	private UnityEvent onDayEnd = new UnityEvent();
	public static UnityEvent OnDayEnd => Current.onDayEnd;

	[Tooltip("Event that fires as the Session begins to end.")]
	[SerializeField]
	private UnityEvent onSessionCleanup = new UnityEvent();
	public static UnityEvent OnSessionCleanup => Current.onSessionCleanup;

	[Tooltip("Event that fires at the end of the Session.")]
	[SerializeField]
	private UnityEvent onSessionEnd = new UnityEvent();
	public static UnityEvent OnSessionEnd => Current.onSessionEnd;

	[Tooltip("Reference to an EncounterPicker object.")]
	[SerializeField]
	private EncounterPicker encounterPicker;
	private static EncounterPicker EncounterPicker => Current.encounterPicker;

	[Space(12)]

	[Tooltip("If true, the Session will end on the next Encounter Clear event.")]
	[SerializeField]
	private bool sessionShouldEnd = false;

	[Tooltip("Storyboard that should queue to end the session.")]
	[SerializeField]
	private Storyboard sessionEndBoard;

	/* Design Note:	The encounter checklist is written to allow systems to check the uniqueness of what
	 *						has been run without having to load a reference to the storyboard itself. It would be
	 *						inefficient to load a storyboard to check if the storyboard should be used. However,
	 *						to avoid the additional maintenence of defining uniqueness in an external data source,
	 *						uniqueness is defined in the storyboard data. So we take advantage of the fact that
	 *						this information is only needed after the first load.
	 */

	/// <summary>Tracks which storyboards have run and whether or not they are unique.</summary>
	private Dictionary<string, bool> encounterChecklist = new Dictionary<string, bool>();
	/// <summary>Reference to the active instance of the checklist that tracks which storyboards have run and whether or not they are unique.</summary>
	private static Dictionary<string, bool> EncounterChecklist => Current.encounterChecklist;
	/// <summary>Check off an encounter as "run" and optionally specify if it is unique.</summary>
	/// <param name="encounterName">The identifying name of the encounter.</param>
	/// <param name="unique">Whether or not this storyboard was unique.</param>
	public static void CheckOffEncounter(string encounterName, bool unique = false) => EncounterChecklist[encounterName] = unique;
	/// <summary>Checks if the given encounter was run and whether or not it was unique.</summary>
	/// <param name="encounterName">The identifying name of the encounter.</param>
	/// <returns>A bool pair where the first indicates whether the encounter was run and the second whether or not it was unique.</returns>
	public static (bool, bool) EncounterWasRun(string encounterName) => 
		EncounterChecklist.ContainsKey(encounterName) 
		? (true, EncounterChecklist[encounterName]) 
		: (false, false);

	/// <summary>The number of days that have completed in the current session.</summary>
	public static int Day { get => Current.day; private set => Current.day = value; }
	private int day = 1;

	/// <summary>
	/// The number of events that have completed on the current game day. Initializes to 0 to represent 
	/// the setup encounter. 
	/// </summary>
	public static int CurrentEncounterNum { get => Current.currentEncounterNum; private set => Current.currentEncounterNum = value; }
	private int currentEncounterNum = 0;

	/// <summary>The total number of encounters that have been completed on a given day.</summary>
	public static int TotalEncounters { get => Current.totalEncounters; private set => Current.totalEncounters = value; }
	private int totalEncounters = 0;

	/// <summary>Given a set of players, make a GameData bundle with the required Session level data.</summary>
	/// <param name="players">The players to retrieve the characters of and put in the bundle.</param>
	/// <returns>A GameData bundle.</returns>
	public static GameData BundleGameData(List<int> players) {
		var characters = CharacterManager.GetPlayerCharacters(players);
		return BundleGameData(characters);
	}

	/// <summary>Given a set of characters, make a GameData bundle with the required Session level data.</summary>
	/// <param name="characters">The characters to put in the bundle.</param>
	/// <returns>A GameData bundle.</returns>
	public static GameData BundleGameData(List<Character> characters) {
		if (Current == null) { return new GameData(characters, new List<string>(), new List<string>()); }
		else return new GameData(characters, EncounterTraits, Traits);
	}

	private void Start() {
		if (Current != this) {
			Destroy(gameObject);
			return;
		}
		encounterPicker = GetComponent<EncounterPicker>();
		Debug.Assert(encounterPicker != null, "No encounter picker found on Session.");

		foreach (var character in CharacterManager.Characters) {
			if (character.Spawned) HandleCharacterSpawned(character);
		}
		CharacterManager.OnPlayerSpawned.AddListener(HandleCharacterSpawned);
	}

	/// <summary>
	/// When a character is spawned, this will subscribe to its OnDeath event to
	/// add the appropriate Session trait.
	/// </summary>
	/// <param name="character">The character who spawned in.</param>
	public void HandleCharacterSpawned(Character character) {
		character.OnDeath.AddListener(HandleCharacterDeath);
	}

	public void HandleCharacterDeath(Character character, List<CharacterStat> limits) {
		foreach (var stat in limits) AddTrait(DeathTraits.From(stat));
		if (CharacterManager.LivingCharacters.Count < 1) {
			AddTrait(partyWipeTrait);
			EndSession();
		}
	}

	/// <summary>
	/// Should be triggered when the Storyboard Queue clears. An encounter represents
	/// a series of storyboards, as one may lead into several others. Not static to support
	/// calls from a UnityEvent.
	/// </summary>
	public void EndEncounter() {
		// Clear out the current encounter's traits.
		EncounterTraits.Clear();

		// Check if Session is set to end. If so, skip normal processing and trigger the final event.
		if (sessionShouldEnd || adventureLength < 1) {
			onSessionEnd.Invoke();
			return;
		}

		onEncounterEnd.Invoke();

		currentEncounterNum++;
		totalEncounters++;

		// If the picker continues to return encounters, continue the session
		var nextEncounter = EncounterPicker.Next();
		if (nextEncounter != null) {
			CheckOffEncounter(nextEncounter);
			StoryboardQueue.Enqueue(nextEncounter);
		}
		// Otherwise prepare to end
		else EndSession();
	}

	/// <summary>Should be triggered when the final encounter of the day has ended.</summary>
	public void EndDay() {
		onDayEnd.Invoke();
		day++;
		currentEncounterNum = 1;
	}

	/// <summary>
	/// Sets the Session to end on the next Encounter end. If present, a closing board will be added 
	/// either to the current encounter, or as a new encounter. Else, it will end immediately.
	/// </summary>
	public void EndSession() {
		sessionShouldEnd = true;
		onSessionCleanup.Invoke();
		if (sessionEndBoard != null) StoryboardQueue.Enqueue(sessionEndBoard);
		else onSessionEnd.Invoke();
	}
}