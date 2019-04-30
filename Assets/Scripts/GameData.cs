using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData {
	[SerializeField]
	private List<Character> characters = new List<Character>();
	public List<Character> Characters => characters;
	[SerializeField]
	private List<string> encounterTraits = new List<string>();
	public List<string> EncounterTraits => encounterTraits;
	[SerializeField]
	private List<string> sessionTraits = new List<string>();
	public List<string> SessionTraits => sessionTraits;

	public GameData(List<Character> characters, List<string> encounterTraits, List<string> sessionTraits) {
		this.characters = characters ?? new List<Character>();
		this.encounterTraits = encounterTraits ?? new List<string>();
		this.sessionTraits = sessionTraits ?? new List<string>();
	}
}