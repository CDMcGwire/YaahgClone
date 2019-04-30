using UnityEngine;

/// <summary>
/// Panel designed to handle player input for a set of options.
/// </summary>
public class DecisionPanel : Panel {
	[Tooltip("Reference to the panel's decision control.")]
	[SerializeField]
	private DecisionControl control;
	public DecisionControl Control => control;
}
