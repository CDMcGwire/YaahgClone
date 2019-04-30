using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Performs string substitutions on a text component to allow for writing in dynamic data.
/// </summary>
public class TextReplaceControl : PanelControl {
	[SerializeField]
	private Text textElement;

	private void OnValidate() {
		Debug.Assert(textElement, "No text element reference has been passed to the Text Replace Control on " + name);
	}

	protected override void Init() {
		if (textElement == null) return;

		var nameString = GenerateNameString(Panel.Board.OwningCharacters);

		var replacement = textElement.text.Replace("{player}", nameString);
		textElement.text = replacement;
	}

	/// <summary>Generate a "name" string based on the number of characters that own the board.</summary>
	/// <param name="characters">The list of owning players.</param>
	/// <returns>The string to use as a "player name" value.</returns>
	private string GenerateNameString(List<Character> characters) {
		if (characters.Count < 1) return "";
		if (CharacterManager.IsParty(characters)) return "The Party";

		var nameBuilder = new StringBuilder(characters[0].Data.Firstname);
		if (characters.Count == 2) {
			nameBuilder.Append(" and ").Append(characters[1].Data.Firstname);
		}
		else if (characters.Count > 1) {
			var last = characters.Count - 1;
			for (var i = 1; i < last; i++) {
				nameBuilder.Append(", ").Append(characters[i].Data.Firstname);
			}
			nameBuilder.Append(", and ").Append(characters[last].Data.Firstname);
		}

		return nameBuilder.ToString();
	}
}