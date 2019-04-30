using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerSpawnEvent : UnityEvent<Character> { }

/// <summary>Singleton object that provides an interface for accessing and instantiating player Character objects.</summary>
public class CharacterManager : MonoBehaviour {
	private static CharacterManager instance;
	public static CharacterManager Instance {
		get {
			if (instance == null) instance = FindObjectOfType<CharacterManager>();
			return instance;
		}
	}

	[SerializeField]
	private Character characterPrefab;

	[SerializeField]
	private PlayerSpawnEvent onPlayerSpawned = new PlayerSpawnEvent();
	public static PlayerSpawnEvent OnPlayerSpawned => Instance.onPlayerSpawned;
	[SerializeField]
	private PlayerSpawnEvent onPlayerRemoved = new PlayerSpawnEvent();
	public static PlayerSpawnEvent OnPlayerRemoved => Instance.onPlayerRemoved;

	[SerializeField]
	private List<Character> characters = new List<Character>();
	/// <summary>Returns a list of all characters.</summary>
	public static List<Character> Characters => Instance.characters;
	/// <summary>Creates a new list containing all currently living characters.</summary>
	public static List<Character> LivingCharacters {
		get {
			var livingCharacters = new List<Character>();
			foreach (var character in Characters) if (character.Data.Alive) livingCharacters.Add(character);
			return livingCharacters;
		}
	}
	/// <summary>Creates a new list containing all currently dead characters.</summary>
	public static List<Character> DeadCharacters {
		get {
			var deadCharacters = new List<Character>();
			foreach (var character in Characters) if (!character.Data.Alive) deadCharacters.Add(character);
			return deadCharacters;
		}
	}

	/// <summary>Convenience method to get a list of player numbers with living characters.</summary>
	public static List<int> LivingPlayers {
		get {
			var players = new List<int>();
			foreach (var character in Characters) {
				if (character.Data.Alive) players.Add(character.PlayerNumber);
			}
			return players;
		}
	}
	/// <summary>Convenience method to get a list of player numbers with dead characters.</summary>
	public static List<int> DeadPlayers {
		get {
			var players = new List<int>();
			foreach (var character in Characters) {
				if (!character.Data.Alive) players.Add(character.PlayerNumber);
			}
			return players;
		}
	}

	/// <summary>Retrieve a Character by player roster index. Makes no distinction on living or dead.</summary>
	/// <param name="player">Roster index of the player to attempt to retrieve.</param>
	/// <returns>The matching player's Character, if it exists.</returns>
	public static Character GetCharacter(int player) {
		if (player < 0 || Instance?.characters == null || player >= Instance.characters.Count)
			return null;
		return Instance.characters[player];
	}

	/// <summary>Retrieve a Character by player roster index if that Character is alive.</summary>
	/// <param name="player">Roster index of the player to attempt to retrieve.</param>
	/// <returns>The matching player's Character, if it exists and is alive.</returns>
	public static Character GetLivingCharacter(int player) {
		var character = GetCharacter(player);
		return character != null && character.Data.Alive ? character : null;
	}

	/// <summary>Retrieve a Character by player roster index if that Character is dead.</summary>
	/// <param name="player">Roster index of the player to attempt to retrieve.</param>
	/// <returns>The matching player's Character, if it exists and is dead.</returns>
	public static Character GetDeadCharacter(int player) {
		var character = GetCharacter(player);
		return character != null && !character.Data.Alive ? character : null;
	}

	/// <summary>Retrieve a set of Characters by player roster number. Makes no distinction on living or dead.</summary>
	/// <param name="players">List of the positional numbers for the players to attempt to retrieve.</param>
	/// <returns>All existing characters that correspond to the given player roster indexes.</returns>
	public static List<Character> GetPlayerCharacters(IList<int> players) {
		var characters = new List<Character>();
		foreach (var player in players) {
			var character = GetCharacter(player);
			if (character != null) characters.Add(character);
		}
		return characters;
	}

	/// <summary>Retrieve a set of living Characters by player roster number.</summary>
	/// <param name="players">List of the positional numbers for the players to attempt to retrieve.</param>
	/// <returns>All living characters that correspond to the given player roster indexes.</returns>
	public static List<Character> GetLivingPlayerCharacters(IList<int> players) {
		var characters = new List<Character>();
		foreach (var player in players) {
			var character = GetLivingCharacter(player);
			if (character != null) characters.Add(character);
		}
		return characters;
	}

	/// <summary>Retrieve a set of dead Characters by player roster number.</summary>
	/// <param name="players">List of the positional numbers for the players to attempt to retrieve.</param>
	/// <returns>All dead characters that correspond to the given player roster indexes.</returns>
	public static List<Character> GetDeadPlayerCharacters(IList<int> players) {
		var characters = new List<Character>();
		foreach (var player in players) {
			var character = GetDeadCharacter(player);
			if (character != null) characters.Add(character);
		}
		return characters;
	}

	/// <summary>Creates a Character game object from the given data and adds to the player roster.</summary>
	/// <param name="player">Roster index to place the character into.</param>
	/// <param name="characterData">Starting statistics to apply to the character.</param>
	/// <param name="deleteOld">If a character already exists in the given index, should it be replaced?</param>
	/// <returns>A reference to the spawned character. Null if an invalid index or a slot collision occurs and deleteOld is false.</returns>
	public static Character SpawnPlayerCharacter(int player, CharacterData characterData, bool deleteOld = true) {
		if (player < 0) return null;

		if (player < Instance.characters.Count) {
			var current = Instance.characters[player];
			if (current) {
				if (deleteOld) Destroy(current.gameObject);
				else return current;
			}
		}

		var character = Instantiate(Instance.characterPrefab, Instance.transform);
		character.PlayerNumber = player;
		character.Data = characterData;

		if (player >= Instance.characters.Count) Instance.characters.Insert(player, character);
		else Instance.characters[player] = character;

		character.SetSpawned();
		OnPlayerSpawned.Invoke(character);
		return character;
	}

	/// <summary>Checks if the list of characters passed contains more than one character and all living characters.</summary>
	/// <param name="owningCharacters">List of characters to display.</param>
	/// <returns>True if the set of living characters contains all characters to display.</returns>
	public static bool IsParty(IList<Character> owningCharacters) {
		if (owningCharacters.Count < 2) return false;
		var owners = new HashSet<Character>(owningCharacters);
		foreach (var character in LivingCharacters)
			if (!owners.Contains(character)) return false;
		return true;
	}

	public void Start() {
		if (Instance != this) {
			Destroy(gameObject);
			return;
		}

		foreach (var character in characters) {
			character.SetSpawned();
			OnPlayerSpawned.Invoke(character);
		}
	}

	/// <summary>Clear out the characters to prepare for next set.</summary>
	public static void Clear() {
		foreach (var character in Characters) Destroy(character.gameObject);
		Characters.Clear();
		OnPlayerSpawned.RemoveAllListeners();
		OnPlayerRemoved.RemoveAllListeners();
	}
}