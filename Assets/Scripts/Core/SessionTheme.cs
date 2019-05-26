using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data class representing the possible encounters in a themed, prodedurally generated Session.
/// Encounters are setup in a tier structure, where the available pool of Storyboards and End of Day events
/// </summary>
[CreateAssetMenu(fileName = "SessionTheme", menuName = "Data/Session Theme")]
public class SessionTheme : ScriptableObject {
	[Tooltip("The encounter group to use on the current highest day threshold reached.")]
	[SerializeField]
	private List<EncounterTier> encounterTiers = new List<EncounterTier>();
	public List<EncounterTier> EncounterTiers => encounterTiers;

	[Tooltip("The encounter group to use on final day of the adventure.")]
	[SerializeField]
	private List<EncounterTier> finalDayTiers = new List<EncounterTier>();
	public List<EncounterTier> FinalDayTiers => finalDayTiers;

	/// <summary>Ensure the lsit is always sorted by Tier. Highest last.</summary>
	private void OnValidate() {
		encounterTiers.Sort((a, b) => a.Rank.CompareTo(b.Rank));
		finalDayTiers.Sort((a, b) => a.Rank.CompareTo(b.Rank));
	}
}

[Serializable]
public class EncounterTier {
	[Tooltip("The number of days that should pass before this tier is reached. 0 for initial.")]
	[SerializeField]
	private int rank = 0;
	public int Rank => rank;

	[Tooltip("The number of encounters to play on that day.")]
	[SerializeField]
	private int numberOfEncounters = 3;
	public int NumberOfEncounters => numberOfEncounters;

	[Tooltip("A list of encounters that can be chosen at this tier.")]
	[SerializeField]
	private List<EncounterListing> encounterPool = new List<EncounterListing>();
	public List<EncounterListing> EncounterPool => encounterPool;

	[Tooltip("The end of day encounter that should be used at this tier.")]
	[SerializeField]
	private List<EncounterListing> dayEndEncounters = new List<EncounterListing>();
	public List<EncounterListing> DayEndEncounters => dayEndEncounters;
}

/// <summary>Mini-class for getting randomly selected storyboards.</summary>
[Serializable]
public class EncounterListing {
	[Tooltip("The relative weighting of the entry.")]
	[SerializeField]
	private int weight = 1;
	public int Weight => weight;

	[Tooltip("The value stored within this entry.")]
	[SerializeField]
	private string uri;
	public string Uri => uri;

	public EncounterListing(int weight, string uri) {
		this.weight = weight;
		this.uri = uri;
	}
	public EncounterListing(string uri) => this.uri = uri;
}