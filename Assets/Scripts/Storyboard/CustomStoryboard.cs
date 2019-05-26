using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Storyboard class to use when manually setting up a Storyboard with custom behaviour.
/// Chains panels together using "Path" components, starting from one on the storyboard
/// itself, and continuing on from Path components on each panel.
/// </summary>
public class CustomStoryboard : Storyboard {
	public override string ID => name;

	[Tooltip("Initial path node that determines the start of the storyboard.")]
	[SerializeField]
	private List<Panel> panels;
	protected override List<Panel> Panels => panels;

	[Tooltip("Should this storyboard only be played once?")]
	[SerializeField]
	private bool unique = false;
	public override bool Unique => unique;

	private int currentPanel = 0;

	protected override Panel NextPanel() => currentPanel >= panels.Count ? null : panels[currentPanel++];
}
