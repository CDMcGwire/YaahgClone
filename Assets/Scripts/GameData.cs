using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData {
	[SerializeField]
	private List<Character> characters = new List<Character>();
	public List<Character> Characters { get { return characters; } }
	[SerializeField]
	private List<string> storyboardTraits = new List<string>();
	public List<string> StoryboardTraits { get { return storyboardTraits; } }
	[SerializeField]
	private List<string> sessionTraits = new List<string>();
	public List<string> SessionTraits { get { return sessionTraits; } }
	
	public GameData(List<Character> characters, List<string> storyboardTraits, List<string> sessionTraits) {
		this.characters = characters ?? new List<Character>();
		this.storyboardTraits = storyboardTraits ?? new List<string>();
		this.sessionTraits = sessionTraits ?? new List<string>();
	}
}