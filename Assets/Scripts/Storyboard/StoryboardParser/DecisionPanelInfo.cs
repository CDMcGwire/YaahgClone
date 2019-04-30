using System.Collections.Generic;

/// <summary>
/// Storyboard Panel which, when played, prompts all listed players
/// </summary>
public class DecisionPanelInfo : PanelInfo {
	public override PanelType Type => PanelType.Decision;

	public string Prompt { get; }
	public List<Decision> Decisions { get; } = new List<Decision>();

	public DecisionPanelInfo(Dictionary<string, string> properties, string prompt, List<Decision> decisions)
		: base(properties) {
		Prompt = prompt;
		Decisions = decisions;
	}

	/// <summary>Information for a data-driven Decision Panel.</summary>
	public struct Decision {
		/// <summary>Text to display on the option.</summary>
		public readonly string text;
		/// <summary>Storyboard URI to queue when chosen.</summary>
		public readonly string next;
		/// <summary>An optional condition to determine if a player can choose this option.</summary>
		public readonly string condition;
		/// <summary>Base intiative order for this option.</summary>
		public readonly int order;

		/// <summary>
		/// Create a new Decision from all available fields.
		/// </summary>
		/// <param name="text">The text to display for the decision.</param>
		/// <param name="next">The URI of the Storyboard to queue on selected.</param>
		/// <param name="condition">The condition to check before enabling the decision.</param>
		/// <param name="order">The relative playback order of the decision.</param>
		public Decision(string text, string next, string condition, int order) {
			this.text = text;
			this.next = next;
			this.condition = condition;
			this.order = order;
		}
	}
}