﻿using System;
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
	public List<EncounterTier> EncounterTiers { get { return encounterTiers; } }

	[Tooltip("The encounter group to use on final day of the adventure.")]
	[SerializeField]
	private List<EncounterTier> finalDayTiers = new List<EncounterTier>();
	public List<EncounterTier> FinalDayTiers { get { return finalDayTiers; } }

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
	public int Rank { get { return rank; } }

	[Tooltip("The number of encounters to play on that day.")]
	[SerializeField]
	private int numberOfEncounters = 3;
	public int NumberOfEncounters { get { return numberOfEncounters; } }

	[Tooltip("A list of encounters that can be chosen at this tier.")]
	[SerializeField]
	private List<EncounterListing> encounterPool = new List<EncounterListing>();
	public List<EncounterListing> EncounterPool { get { return encounterPool; } }

	[Tooltip("The end of day encounter that should be used at this tier.")]
	[SerializeField]
	private List<EncounterListing> dayEndEncounters = new List<EncounterListing>();
	public List<EncounterListing> DayEndEncounters { get { return dayEndEncounters; } }
}

/// <summary>Mini-class for getting randomly selected storyboards.</summary>
[Serializable]
public class EncounterListing {
	/// <summary>The relative weighting of the entry.</summary>
	[SerializeField]
	private int weight = 1;
	public int Weight { get { return weight; } }
	/// <summary>The value stored within this entry.</summary>
	[SerializeField]
	private Storyboard value;
	public Storyboard Value { get { return value; } }

	public EncounterListing(int weight, Storyboard value) {
		this.weight = weight;
		this.value = value;
	}
	public EncounterListing(Storyboard value) {
		this.value = value;
	}
}