using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A panel control that automatically checks a set conditional branch events and triggers
/// for the owning players.
/// </summary>
public class BranchControl : PanelControl {
	[Tooltip("Possible branches for characters to take upon reaching the panel. Will be resolved in order. For default branch, create a zero condition branch at the end of the list.")]
	[SerializeField]
	private List<ConditionalBranch> conditionalBranches = new List<ConditionalBranch>();
	public List<ConditionalBranch> ConditionalBranches { get => conditionalBranches; set => conditionalBranches = value; }

	[Tooltip("Should this branch be able to split the party?")]
	[SerializeField]
	private bool canSplitParty = true;
	public bool CanSplitParty { get => canSplitParty; set => canSplitParty = value; }

	/// <summary>
	/// Upon initialization, immediately determine which branches the players should take, if any.
	/// Once all players are determined and grouped, fire off the associated events. If "canSplitParty"
	/// is enabled, then players are evaluated individually. Otherwise, the conditions and events
	/// will act on the owning group as a whole.
	/// 
	/// If a player or group does not match any condition, no events will fire for them.
	/// </summary>
	protected override void Init() {
		var owners = Panel.Board.OwningPlayers;
		var fork = new NarrativeFork();

		if (canSplitParty) {
			foreach (var player in owners) {
				var branch = ResolveBranchCondition(new List<int>{ player });
				if (branch != null) fork.PickBranch(player, branch);
			}
		}
		else {
			var branch = ResolveBranchCondition(owners);
			if (branch != null) fork.PickBranch(owners, branch);
		}

		fork.SendOff();
	}

	/// <summary>Determine which branch the list of players should take.</summary>
	/// <param name="players">The players to which the branch applies.</param>
	private Branch ResolveBranchCondition(List<int> players) {
		var gameData = Session.BundleGameData(players);
		var parser = new ConditionParser(gameData);

		foreach (var branch in conditionalBranches) {
			if (parser.Evaluate(branch.Condition)) return branch.Data;
		}
		return null;
	}
}

/// <summary>
/// Data representing a single traversable branch for a BranchControl.
/// </summary>
[Serializable]
public struct ConditionalBranch {
	/// <summary>Condition under which this branch applies.</summary>
	public string Condition;
	/// <summary>Branch data.</summary>
	public Branch Data;
}