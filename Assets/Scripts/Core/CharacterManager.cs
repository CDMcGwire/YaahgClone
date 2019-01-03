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
	public static PlayerSpawnEvent OnPlayerSpawned { get { return Instance.onPlayerSpawned; } }
	[SerializeField]
	private PlayerSpawnEvent onPlayerRemoved = new PlayerSpawnEvent();
	public static PlayerSpawnEvent OnPlayerRemoved { get { return Instance.onPlayerRemoved; } }

	[SerializeField]
	private List<Character> characters = new List<Character>();
	public static List<Character> Characters { get { return Instance.characters; } }

	/// <summary>Retrieve a Character by player roster index. Makes no distinction on living or dead.</summary>
	/// <param name="player">Roster index of the player to attempt to retrieve.</param>
	/// <returns>The matching player's Character, if it exists.</returns>
	public static Character GetCharacter(int player) {
		if (player < 0 || player >= Instance.characters.Count) return null;
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

	/// <returns>Retrieve a set of player roster indexes where each corresponding player has a living character.</returns>
	public static List<int> GetLivingPlayers() {
		var players = new List<int>();
		foreach (var character in Characters) {
			if (character.Data.Alive) players.Add(character.PlayerNumber);
		}
		return players;
	}

	/// <returns>Retrieve a set of player roster indexes where each corresponding player has a dead character.</returns>
	public static List<int> GetDeadPlayers() {
		var players = new List<int>();
		foreach (var character in Characters) {
			if (!character.Data.Alive) players.Add(character.PlayerNumber);
		}
		return players;
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
		character.Data = characterData;

		if (player >= Instance.characters.Count) Instance.characters.Insert(player, character);
		else Instance.characters[player] = character;
		
		OnPlayerSpawned.Invoke(character);
		return character;
	}

	private void Start() {
		foreach (var character in characters) {
			OnPlayerSpawned.Invoke(character);
		}
	}
}