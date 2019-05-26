using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Picks the next storyboard in a procedural fashion based on the current progress of the Session in days.
/// Each tier has a pool of 
/// </summary>
public class ProceduralEncounterPicker : EncounterPicker {
	[Tooltip("Reference to the scriptable object containing the data to pick from.")]
	[SerializeField]
	private SessionTheme sessionTheme;
	public SessionTheme SessionTheme => sessionTheme;

	private int currentTier = 0;

	private void OnValidate() {
		if (SessionTheme == null) {
			Debug.LogWarning("Missing Theme on Procedural Encounter Picker \"" + name + "\"");
		}
		else if (SessionTheme.EncounterTiers.Count < 1) {
			Debug.LogWarning("Missing Encounter Tiers on Theme \"" + SessionTheme.name + "\"");
		}
	}

	/// <summary>
	/// Parses the ThemedSession to get the next Storyboard to play. Automatically increments state.
	/// </summary>
	public override string Next() {
		var currentEncounterCap = SessionTheme.EncounterTiers[currentTier].NumberOfEncounters;
		if (Session.CurrentEncounterNum > currentEncounterCap) Session.Current.EndDay();

		var finalDay = Session.Day == Session.AdventureLength;
		var tierList = finalDay ? SessionTheme.FinalDayTiers : SessionTheme.EncounterTiers;

		// If switching tier list on this run, ensure the tier doesn't exceed the number available.
		if (currentTier >= tierList.Count) currentTier = tierList.Count - 1;
		// If there are more tiers and the next Rank threshold has been reached, advance to the next tier
		else if (currentTier + 1 < tierList.Count && tierList[currentTier + 1].Rank <= Session.Day) currentTier++;

		var tier = tierList[currentTier];
		if (Session.CurrentEncounterNum < tier.NumberOfEncounters) {
			return PickStoryboard(tier.EncounterPool);
		}
		else if (Session.CurrentEncounterNum == tier.NumberOfEncounters) {
			// If there are Day End storyboards available, choose from there, else grab a normal one.
			return PickStoryboard(tier.DayEndEncounters.Count > 0 ? tier.DayEndEncounters : tier.EncounterPool);
		}

		// No more storyboards to run
		return null;
	}

	/// <summary>Returns a weighted Random storyboard from the chosen pool.</summary>
	/// <param name="weightedValues">A list of storyboards with weight values.</param>
	/// <returns>A storyboard from the pool.</returns>
	private string PickStoryboard(List<EncounterListing> weightedValues) {
		// Get the total weight of entries that can still be run this Session
		var totalWeight = 0;
		var availableEntries = new List<EncounterListing>();
		foreach (var entry in weightedValues) {
			if (entry.Uri == null) {
				Debug.LogError("Procedural Encounter Picker \"" + sessionTheme.name + "\" has a missing storyboard value.");
			}
			// If the storyboard is not unique or has not been run, count it towards the available entries
			else {
				var (wasRun, unique) = Session.EncounterWasRun(entry.Uri);
				if (!wasRun || !unique) {
					totalWeight += entry.Weight;
					availableEntries.Add(entry);
				}
			}
		}

		// Choose one of the available entries to run
		var target = Random.Range(0, totalWeight);
		foreach (var entry in availableEntries) {
			if (target < entry.Weight) return entry.Uri;
			else target -= entry.Weight;
		}

		// No entries available
		return null;
	}
}