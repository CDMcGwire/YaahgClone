using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>An event triggered from a Narrative Branch. Passes the list of traversing players.</summary>
[Serializable]
public class BranchEvent : UnityEvent<List<int>> { }

/// <summary>
/// A class for automatically resolving grouping and ordering for a series of Narrative Branches.
/// Call PickGroup once for each player/branch pair that needs to be considered for the fork, then
/// call SendOff to fire off the ordered and grouped events.
/// </summary>
public class NarrativeFork {
	/// <summary>Map of branches that have been picked.</summary>
	private Dictionary<string, BranchGroup> activeBranches = new Dictionary<string, BranchGroup>();

	/// <summary>Add player to the active branches.</summary>
	/// <param name="player">The roster index of the player.</param>
	/// <param name="branch">The branch data to pair with.</param>
	/// <param name="merge">If the branch has been picked before, should this player join that group?</param>
	public void PickBranch(int player, Branch branch, bool merge = true) => PickBranch(new List<int> { player }, branch, merge);

	/// <summary>Add a list of players to the active branches.</summary>
	/// <param name="players">The roster indices of the players.</param>
	/// <param name="branch">The branch data to pair with.</param>
	/// <param name="merge">If the branch has been picked before, should this player join that group?</param>
	public void PickBranch(List<int> players, Branch branch, bool merge = true) {
		if (merge) {
			var key = branch.ID;
			if (activeBranches.ContainsKey(key)) {
				activeBranches[key].Players.Merge(players);
			}
			else {
				activeBranches[key] = new BranchGroup(players, branch);
			}
		}
		else {
			var group = new BranchGroup(players, branch);
			var randomKey = branch.ID + UnityEngine.Random.Range(0, int.MaxValue);
			activeBranches[randomKey] = group;
		}
	}

	/// <summary>
	/// Fire off all the picked branches in initiative order. 
	/// All active branches are cleared in the process.
	/// </summary>
	public void SendOff() {
		if (activeBranches.Count < 1) return;
		var sortedBranches = new SortedList<int, BranchGroup>(new InitiativeComparer());
		foreach (var group in activeBranches.Values) {
			sortedBranches.Add(group.FinalPlaybackOrder, group);
		}
		foreach (var group in sortedBranches.Values) {
			group.Branch.OnBranch.Invoke(group.Players);
		}
		activeBranches.Clear();
	}

	/// <summary>Maintains a branch reference with the traversing players.</summary>
	private struct BranchGroup {
		public List<int> Players { get; }
		public Branch Branch { get; }

		public BranchGroup(List<int> players, Branch branch) {
			Branch = branch;
			Players = players;
		}

		/// <summary>Calculates playback order using group initiative.</summary>
		public int FinalPlaybackOrder {
			get {
				var sum = 0;
				var characters = CharacterManager.GetPlayerCharacters(Players);
				foreach (var character in characters) sum += character.Data.Initiative;

				var groupSize = characters.Count;
				var averageInitiative = Mathf.RoundToInt(sum / (float)groupSize);
				var groupSizeBonus = (groupSize - 1) / 2;

				var finalInitiative = averageInitiative + groupSizeBonus;
				return Branch.PlaybackOrder - finalInitiative;
			}
		}
	}

	/// <summary>IComparer implementation for sorting decision objects.</summary>
	private class InitiativeComparer : IComparer<int> {
		public int Compare(int x, int y) {
			var result = x.CompareTo(y);
			return result == 0 ? -1 : result;
		}
	}
}

/// <summary>Holds all the relevant information needed for ordering and triggering a branch event.</summary>
[Serializable]
public class Branch {
	/// <summary>Hash ID generated upon first request.</summary>
	private string id;
	public string ID {
		get {
			if (id == null) id = UnityEngine.Random.Range(1, int.MaxValue).ToString();
			return id;
		}
	}
	/// <summary>The base initiative order for this branch.</summary>
	public int PlaybackOrder = 0;
	/// <summary>The event to fire when traversed.</summary>
	public BranchEvent OnBranch = new BranchEvent();

	public Branch() { }

	/// <summary>Instantiate a Branch with an immediate listener.</summary>
	/// <param name="order">Playerback order of the branch.</param>
	/// <param name="listener">The invokeable listener.</param>
	public Branch(int order, UnityAction<List<int>> listener) {
		PlaybackOrder = order;
		OnBranch.AddListener(listener);
	}
}