using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A panel which, when played, will automatically fire a set of conditional events for each owning player.
/// </summary>
public class BranchPanel : Panel {
	[Tooltip("Reference to the branch control component.")]
	[SerializeField]
	private BranchControl branchControl;
	public BranchControl BranchControl => branchControl;
}