using System.Collections.Generic;
/// <summary>
/// Base class for PanelInfo objects, which contain the data needed to run a particular type of panel.
/// </summary>
public abstract class PanelInfo {
	/// <summary>Implemented by child class to help determine the derived type.</summary>
	public abstract PanelType Type { get; }
	public IReadOnlyDictionary<string, string> Properties { get; }

	public PanelInfo(Dictionary<string, string> properties) {
		Properties = properties;
	}
}

/// <summary>
/// Represents possible DataDriven panel types.
/// </summary>
public enum PanelType {
	Narrative,
	Decision,
	Branch
}