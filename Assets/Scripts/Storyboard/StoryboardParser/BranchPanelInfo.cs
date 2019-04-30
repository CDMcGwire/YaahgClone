using System;
using System.Collections.Generic;

/// <summary>
/// Storyboard panel representing an automatic branch in the narrative.
/// </summary>
public class BranchPanelInfo : PanelInfo {
	public override PanelType Type => PanelType.Branch;

	public List<Data> Branches { get; } = new List<Data>();

	public BranchPanelInfo(Dictionary<string, string> properties, List<Data> branches)
		: base(properties) {
		Branches = branches;
	}

	[Serializable]
	public struct Data {
		/// <summary>The URI for the next storyboard to play.</summary>
		public string next;
		/// <summary>The condition under which the branch is taken.</summary>
		public string condition;
		/// <summary>The base initiative order for the branch.</summary>
		public int playbackOrder;

		/// <summary>
		/// Creates a data object from the constituent parts.
		/// </summary>
		/// <param name="next">The URI for the next storyboard to play.</param>
		/// <param name="condition">The condition under which the branch is taken.</param>
		/// <param name="playbackOrder">The base initiative order for the branch.</param>
		public Data(string next, string condition, int playbackOrder) {
			this.next = next;
			this.condition = condition;
			this.playbackOrder = playbackOrder;
		}
	}
}