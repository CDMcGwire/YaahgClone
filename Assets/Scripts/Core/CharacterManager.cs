using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerSpawnEvent : UnityEvent<Character> { }

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

	[SerializeField]
	private List<Character> deadCharacters = new List<Character>();
	public static List<Character> DeadCharacters { get { return Instance.deadCharacters; } }

	public static Character GetLivingCharacter(int player) {
		if (player < 0 || player >= Instance.characters.Count) return null;
		return Instance.characters[player];
	}

	public static Character GetDeadCharacter(int player) {
		if (player < 0 || player >= Instance.deadCharacters.Count) return null;
		return Instance.deadCharacters[player];
	}

	public static List<Character> GetLivingPlayerCharacters(IList<int> players) {
		var characters = new List<Character>();
		foreach (var player in players) {
			var character = GetLivingCharacter(player);
			if (character != null) characters.Add(character);
		}
		return characters;
	}

	public static List<Character> GetDeadPlayerCharacters(IList<int> players) {
		var characters = new List<Character>();
		foreach (var player in players) {
			var character = GetDeadCharacter(player);
			if (character != null) characters.Add(character);
		}
		return characters;
	}

	public static List<int> GetLivingPlayers() {
		var players = new List<int>();
		foreach (var character in Characters) players.Add(character.PlayerNumber);
		return players;
	}

	public static List<int> GetDeadPlayers() {
		var players = new List<int>();
		foreach (var character in DeadCharacters) players.Add(character.PlayerNumber);
		return players;
	}

	public static Character SpawnPlayerCharacter(int player, CharacterData characterInfo, bool deleteOld = true) {
		if (player < 0) return null;

		if (player < Instance.characters.Count) {
			var current = Instance.characters[player];
			if (current) {
				if (deleteOld) Destroy(current.gameObject);
				else return current;
			}
		}

		var character = Instantiate(Instance.characterPrefab, Instance.transform);
		character.Data = characterInfo;

		if (player >= Instance.characters.Count) Instance.characters.Insert(player, character);
		else Instance.characters[player] = character;

		character.OnDeath.AddListener(HandleDyingCharacter);
		OnPlayerSpawned.Invoke(character);
		return character;
	}

	private static void HandleDyingCharacter(Character character, List<CharacterStat> limitsReached) {
		if (!Characters.Contains(character)) return;
		Characters.Remove(character);
		if (DeadCharacters.Contains(character)) return;
		DeadCharacters.Add(character);
	}

	private void Start() {
		foreach (var character in characters) {
			character.OnDeath.AddListener(HandleDyingCharacter);
			OnPlayerSpawned.Invoke(character);
		}
	}
}