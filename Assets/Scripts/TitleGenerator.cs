using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a handle to the list of available titles for use in getting a random character title.
/// </summary>
[CreateAssetMenu(fileName = "TitleGenerator", menuName = "Data/Title Generator")]
public class TitleGenerator : ScriptableObject {
	[SerializeField]
	private TextAsset titleFile;

	private List<CharacterData> titles = new List<CharacterData>();

	public CharacterData Next {
		get {
			if (titles.Count < 1) LoadTitles();
			return titles[Random.Range(0, titles.Count)];
		}
	}

	private void LoadTitles() {
		var lines = titleFile.text.Split('\n');
		for (var i = 1; i < lines.Length; i++) { // Skip header line
			var fields = lines[i].Split(',');
			var data = new CharacterData {
				Title = fields[0],
				Strength = int.Parse(fields[1]),
				Toughness = int.Parse(fields[2]),
				Dexterity = int.Parse(fields[3]),
				Perception = int.Parse(fields[4]),
				Magic = int.Parse(fields[5]),
				Knowledge = int.Parse(fields[6]),
				Charisma = int.Parse(fields[7]),
				Willpower = int.Parse(fields[8]),
				Lethality = int.Parse(fields[9]),
				Money = int.Parse(fields[10]),
				PhysicalThreshold = int.Parse(fields[11]),
				MentalThreshold = int.Parse(fields[12]),
				DebtThreshold = int.Parse(fields[13])
			};
			// Add the remaining fields as titles if they're not empty.
			for (var j = 14; j < fields.Length; j++) {
				var trait = fields[j];
				if (!string.IsNullOrWhiteSpace(trait)) data.AddTrait(trait);
			}
			titles.Add(data);
		}
	}
}
