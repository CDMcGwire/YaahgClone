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
	public static List<string> Traits { get { return Current?.traits; } }

	[Tooltip("Trait added on all players dead.")]
	[SerializeField]
	private string partyWipeTrait = "Party Wipe";
	public static string PartyWipeTrait { get { return Current.partyWipeTrait; } }

	[Space(12)]

	[Tooltip("How long the adventure should last in days.")]
	[SerializeField]
	private int adventureLength = 5;
	public static int AdventureLength { get { return Current.adventureLength; } set { Current.adventureLength = value; } }

	[Tooltip("Event that fires at the end of an Encounter.")]
	[SerializeField]
	private UnityEvent onEncounterEnd = new UnityEvent();
	public static UnityEvent OnEncounterEnd { get { return Current.onEncounterEnd; } }

	[Tooltip("Event that fires at the end of a game day.")]
	[SerializeField]
	private UnityEvent onDayEnd = new UnityEvent();
	public static UnityEvent OnDayEnd { get { return Current.onDayEnd; } }

	[Tooltip("Event that fires as the Session begins to end.")]
	[SerializeField]
	private UnityEvent onSessionCleanup = new UnityEvent();
	public static UnityEvent OnSessionCleanup { get { return Current.onSessionCleanup; } }

	[Tooltip("Event that fires at the end of the Session.")]
	[SerializeField]
	private UnityEvent onSessionEnd = new UnityEvent();
	public static UnityEvent OnSessionEnd { get { return Current.onSessionEnd; } }

	[Tooltip("Reference to an EncounterPicker object.")]
	[SerializeField]
	private EncounterPicker encounterPicker;
	private static EncounterPicker EncounterPicker { get { return Current.encounterPicker; } }

	[Space(12)]

	[Tooltip("If true, the Session will end on the next Encounter Clear event.")]
	[SerializeField]
	private bool sessionShouldEnd = false;

	[Tooltip("Storyboard that should queue to end the session.")]
	[SerializeField]
	private Storyboard sessionEndBoard;

	/// <summary>Tracks which storyboards have run, so Unique ones can be ignored.</summary>
	private HashSet<string> encounterChecklist = new HashSet<string>();
	public static void CheckOffEncounter(string encounterName) { Current.encounterChecklist.Add(encounterName); }
	public static bool EncounterWasRun(string encounterName) { return Current.encounterChecklist.Contains(encounterName); }

	/// <summary>The number of days that have completed in the current session.</summary>
	public static int Day { get { return Current.day; } private set { Current.day = value; } }
	private int day = 1;

	/// <summary>
	/// The number of events that have completed on the current game day. Initializes to 0 to represent 
	/// the setup encounter. 
	/// </summary>
	public static int CurrentEncounterNum { get { return Current.currentEncounterNum; } private set { Current.currentEncounterNum = value; } }
	private int currentEncounterNum = 0;

	/// <summary>The total number of encounters that have been completed on a given day.</summary>
	public static int TotalEncounters { get { return Current.totalEncounters; } private set { Current.totalEncounters = value; } }
	private int totalEncounters = 0;

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

	/// <summary>Adds the given trait to the list of Session traits.</summary>
	/// <param name="trait">Trait identifying string.</param>
	public static void AddTrait(string trait) {
		if (Current == null) return;
		if (Traits.Contains(trait)) return;
		Traits.Add(trait);
	}

	/// <param name="trait">Trait to lookup.</param>
	/// <returns>True if found.</returns>
	public static bool HasTrait(string trait) {
		if (Current == null) return false;
		return Traits.Contains(trait);
	}

	/// <summary>
	/// Should be triggered when the Storyboard Queue clears. An encounter represents
	/// a series of storyboards, as one may lead into several others. Not static to support
	/// calls from a UnityEvent.
	/// </summary>
	public void EndEncounter() {
		// Check if Session is set to end. If so, skip normal processing and trigger the final event.
		if (sessionShouldEnd) {
			onSessionEnd.Invoke();
			return;
		}

		onEncounterEnd.Invoke();

		currentEncounterNum++;
		totalEncounters++;

		// If the picker continues to return encounters, continue the session
		var nextEncounter = EncounterPicker.Next;
		if (nextEncounter != null) {
			CheckOffEncounter(nextEncounter.name);
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