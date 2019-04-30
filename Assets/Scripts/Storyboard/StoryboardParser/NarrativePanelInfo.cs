using System;
using System.Collections.Generic;

/// <summary>
/// A panel which, when played, triggers a timed, skippable text narrative.
/// During which, a change to the game condition may occur.
/// </summary>
public class NarrativePanelInfo : PanelInfo {
	public override PanelType Type => PanelType.Narrative;

	public string Text { get; }
	public NarrativeEffect Effect { get; }

	public NarrativePanelInfo(Dictionary<string, string> properties, string text, NarrativeEffect effect) 
		: base(properties) {
		Text = text;
		Effect = effect;
	}
}

/// <summary>
/// Defines all of the changes that can occur when a Narrative panel is played.
/// </summary>
[Serializable]
public struct NarrativeEffect {
	public List<StatChange> statChanges;
	public List<string> characterTraits;
	public List<string> encounterTraits;
	public List<string> sessionTraits;
}