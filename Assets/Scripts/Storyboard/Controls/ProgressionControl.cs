using System.Collections.Generic;
using UnityEngine;

public class ProgressionControl : PanelControl {
	[Header("Character Changes")]
	
	[Tooltip("Stat changes to apply to each character when applied.")]
	[SerializeField]
	private List<StatChange> statChanges = new List<StatChange>();

	[Tooltip("Traits to add to each character when applied.")]
	[SerializeField]
	private List<string> traits = new List<string>();

	[Space(12)]
	[Header("Game State Changes")]

	[Tooltip("Traits to add to the encounter when applied.")]
	[SerializeField]
	private List<string> encounterTraits = new List<string>();

	[Tooltip("Traits to add to the session when applied.")]
	[SerializeField]
	private List<string> sessionTraits = new List<string>();

	[SerializeField]
	[HideInInspector]
	private bool applied = false;

	/// <summary>Applies the list of trait additions and stat changes unless already applied.</summary>
	public void Apply() {
		if (applied) return;
		applied = true;

		foreach (var character in Panel.Board.OwningCharacters) {
			if (statChanges.Count > 0) character.ChangeStats(statChanges);
			foreach (var trait in traits) { character.Data.Traits.Add(trait); }
		}
		foreach (var trait in encounterTraits) Session.AddEncounterTrait(trait);
		foreach (var trait in sessionTraits) Session.AddTrait(trait);
	}

	/// <summary>Update the list of effects that should be applied.</summary>
	/// <param name="narrativeEffect">Struct containing the new set of effects to apply.</param>
	public void Set(NarrativeEffect narrativeEffect) {
		statChanges = narrativeEffect.statChanges ?? new List<StatChange>();
		traits = narrativeEffect.characterTraits ?? new List<string>();
		encounterTraits = narrativeEffect.encounterTraits ?? new List<string>();
		sessionTraits = narrativeEffect.sessionTraits ?? new List<string>();
	}
}
