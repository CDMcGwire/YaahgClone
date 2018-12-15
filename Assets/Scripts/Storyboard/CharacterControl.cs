using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : PanelControl {
	[SerializeField]
	private List<string> traits = new List<string>();

	[SerializeField]
	private List<StatChange> statChanges = new List<StatChange>();

	[SerializeField]
	[HideInInspector]
	private bool applied = false;

	public void Apply() {
		if (applied) return;
		applied = true;
		foreach (var character in Panel.Board.OwningCharacters) {
			character.ChangeStats(statChanges);
			foreach (var trait in traits) { character.Data.AddTrait(trait); }
		}
	}
}
