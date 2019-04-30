using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Storyboard class to use when manually setting up a Storyboard with custom behaviour.
/// Chains panels together using "Path" components, starting from one on the storyboard
/// itself, and continuing on from Path components on each panel.
/// </summary>
public class CustomStoryboard : Storyboard {
	/// <summary>Initial path node that determines the start of the storyboard.</summary>
	[SerializeField]
	private List<Panel> panels;
	protected override List<Panel> Panels => panels;

	private int currentPanel = 0;

	protected override Panel NextPanel() => currentPanel >= panels.Count ? null : panels[currentPanel++];
}
