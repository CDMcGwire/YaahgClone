using System.Collections.Generic;

/// <summary>
/// Holds a reference to all the dynamic data loaded for the storyboard.
/// </summary>
public class StoryboardData {
	public readonly string ID;
	public readonly IReadOnlyList<PanelInfo> Panels;
	public readonly IReadOnlyDictionary<string, string> Properties;

	public StoryboardData(string id, List<PanelInfo> panels, Dictionary<string, string> properties) {
		ID = id;
		Panels = panels;
		Properties = properties;
	}
}
