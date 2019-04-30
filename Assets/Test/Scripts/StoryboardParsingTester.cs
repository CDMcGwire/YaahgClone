using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Test class for running the storyboard parser on its own.
/// </summary>
public class StoryboardParsingTester : MonoBehaviour {
	public TextAsset loadableStoryboardData;
	public Text outputElement;

	/// <summary>
	/// Attempts to load a storyboard set from the given data file and output information about the result.
	/// </summary>
	public void LoadStoryboardDataFromAsset() {
		var storyboardParser = new StoryboardParser();
		var storyboards = storyboardParser.Parse(loadableStoryboardData);

		var outputBuilder = new StringBuilder("Parsed Storyboards:");
		foreach (var entry in storyboards) {
			outputBuilder.AppendLine($"Parsed storyboard {entry.Key}");
			foreach (var property in entry.Value.Properties)
				outputBuilder.AppendLine($"Storyboard Property: {property.Key}={property.Value}");

			var panelCount = 1;
			foreach (var panel in entry.Value.Panels) {
				outputBuilder.AppendLine($"Parsed Panel {panelCount}");
				outputBuilder.AppendLine($"\tType = {panel.Type.ToString()}");
				foreach (var property in panel.Properties)
					outputBuilder.AppendLine($"\tProperty: {property.Key}={property.Value}");
			}
		}

		outputElement.text = outputBuilder.ToString();
	}
}
