using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Parses a data file into a series of Panel Sets which can be played on a Storyboard.
/// </summary>
public sealed class StoryboardParser {
	private static class Symbol {
		// Chars are const to allow for more consistent behaviour as chars
		public const char PropertyChar = '&';
		public const char AddChar = '+';
		public const char SubChar = '-';
		public const char SetChar = '=';
		// Strings are static readonly to allow for dynamic initialization without mutation
		public static readonly string ConditionOpen = "[";
		public static readonly string ConditionClose = "]";
		public static readonly string LabelLine = ">>";
		public static readonly string CommentLine = "#";
		public static readonly string BoardProperty = "^" + PropertyChar;
		public static readonly string PanelProperty = PropertyChar.ToString();
		public static readonly string PropertyDelim = "=";
		public static readonly string NarrativeLine = "|";
		public static readonly string PromptLine = "?";
		public static readonly string BranchLine = "-";
		public static readonly string EffectLine = ":";
		public static readonly string Encounter = "encounter";
		public static readonly string Session = "session";
	}

	private static readonly IReadOnlyDictionary<string, CharacterStat> StatMap = new Dictionary<string, CharacterStat>{
		{ "strength", CharacterStat.STR },
		{ "str", CharacterStat.STR },
		{ "toughness", CharacterStat.TUF },
		{ "tuf", CharacterStat.TUF },
		{ "dexterity", CharacterStat.DEX },
		{ "dex", CharacterStat.DEX },
		{ "perception", CharacterStat.PER },
		{ "per", CharacterStat.PER },
		{ "magic", CharacterStat.MAG },
		{ "mag", CharacterStat.MAG },
		{ "knowledge", CharacterStat.KNO },
		{ "kno", CharacterStat.KNO },
		{ "willpower", CharacterStat.WIL},
		{ "wil", CharacterStat.WIL },
		{ "charisma", CharacterStat.CHA },
		{ "cha", CharacterStat.CHA },
		{ "swagger", CharacterStat.CHA },
		{ "madlad", CharacterStat.CHA },
		{ "lethality", CharacterStat.LET },
		{ "let", CharacterStat.LET },
		{ "money", CharacterStat.MONEY },
		{ "dough", CharacterStat.MONEY }
	};
	private static Regex StatRegex { get; } = new Regex(@"([a-z]+)\s*([+=-])\s*(-?[0-9]+)", RegexOptions.IgnoreCase);
	private static Regex TraitRegex { get; } = new Regex(@"([a-z]+)?\s*\+\s*""([^""]*)""", RegexOptions.IgnoreCase);
	private static Regex DecisionRegex { get; } = new Regex(@"-\s*(\d+)\s*-(.*?)-<(.*)", RegexOptions.IgnoreCase);
	private static Regex BranchRegex { get; } = new Regex(@"-\s*(\d+)\s*-<(.*)", RegexOptions.IgnoreCase);

	/// <summary>The max number of lines allowed for a single narration panel in the next parse call.</summary>
	private int MaxNarrationLines { get; set; } = 3;
	/// <summary>The max number of lines allowed for a single decision prompt in the next parse call.</summary>
	private int MaxPromptLines { get; set; } = 2;

	/// <summary>
	/// Parse a given TextAsset into a set of runnable storyboards.
	/// </summary>
	/// <returns>A Dictionary of storyboard data objects keyed by name.</returns>
	public Dictionary<string, StoryboardData> Parse(TextAsset textAsset) {
		var parseInfo = new ParseContext(
			textAsset.name,
			textAsset.text,
			MaxNarrationLines,
			MaxPromptLines
		);
		return parseInfo.Process();
	}

	private struct NumberedLine {
		public int Number { get; }
		public string Text { get; }

		public NumberedLine(int number, string text) {
			Number = number;
			Text = text;
		}
	}

	private struct ParseContext {

		private string AssetName { get; }
		private string RawText { get; }
		private int NarrationLines { get; }
		private int PromptLines { get; }

		public ParseContext(string assetName, string rawText, int narrationLines, int promptLines) {
			AssetName = assetName;
			RawText = rawText;
			NarrationLines = narrationLines;
			PromptLines = promptLines;
		}

		private void WarnTooManyInitialStoryboards(NumberedLine line) => Debug.LogWarning(
			$"The storyboard data asset \"{AssetName}\" has an extra initial storyboard on line ${line.Number}. Only one nameless entry is allowed per file."
		);

		private void WarnInvalidProperty(NumberedLine line) => Debug.LogWarning(
			$"The storyboard data asset \"{AssetName}\" Contains an invalid property on line {line.Number} \"{line.Text}\". Must be of format {{key}}={{value}}."
		);

		private void WarnInvalidStat(string invalidStat, List<int> lineNumbers) => Debug.LogWarning(
			$"Invalid stat \"{invalidStat}\" was found on lines {string.Join(", ", lineNumbers)} when processing file \"{AssetName}\""
		);

		private void WarnInvalidTrait(string invalidTrait, List<int> lineNumbers) => Debug.LogWarning(
			$"Invalid trait target \"{invalidTrait}\" was found on lines {string.Join(", ", lineNumbers)} when processing file \"{AssetName}\""
		);

		private void WarnTooManyPromptLines(int lineNumber) => Debug.LogWarning(
			$"Too many prompt lines found in storyboard data asset \"{AssetName}\" starting at line {lineNumber}. Maximum prompt lines = {PromptLines}"
		);

		/// <summary>
		/// Does some quick transformations to the file to make it more machine readable.
		/// </summary>
		/// <param name="rawLines">The original set of lines from the asset file.</param>
		/// <returns>A queue of the processable lines.</returns>
		private Queue<NumberedLine> Preprocess(string[] rawLines) {
			var lines = new Queue<NumberedLine>();
			for (var i = 0; i < rawLines.Length; i++) {
				var line = rawLines[i].Trim();
				// Throw out comments
				if (rawLines[i].StartsWith(Symbol.CommentLine)) continue;
				// Throw out extra empty lines
				else if (string.IsNullOrWhiteSpace(line) && i > 0) {
					var lastLine = rawLines[i - 1];
					if (string.IsNullOrWhiteSpace(lastLine)) continue;
				}
				lines.Enqueue(new NumberedLine(i, rawLines[i].Trim()));
			}
			return lines;
		}

		/// <summary>
		/// Turn the lines of a Storyboard Data Asset into a dictionary of individual Storyboard Data objects.
		/// </summary>
		/// <param name="lines">A queue of text lines from the asset file.</param>
		/// <returns>A dictionary of storyboard data objects keyed by their name.</returns>
		public Dictionary<string, StoryboardData> Process() {
			var storyboards = new Dictionary<string, StoryboardData>();
			var lines = Preprocess(RawText.Split('\n'));

			while (lines.Count > 0) {
				var next = lines.Dequeue();
				var trimLine = next.Text.TrimStart();
				if (trimLine.StartsWith(Symbol.LabelLine)) {
					var name = ParseBoardname(trimLine);
					if (!storyboards.ContainsKey(name)) storyboards[name] = ProcessStoryboard(lines, name);
					else WarnTooManyInitialStoryboards(next);
				}
			}
			return storyboards;
		}

		/// <summary>
		/// Parses the starting line of a Storyboard to get the name.
		/// </summary>
		/// <param name="line">The line of data containing the storyboard name.</param>
		/// <returns>The parsed name if possible, else "initial".</returns>
		private string ParseBoardname(string line) {
			var name = line.Substring(Symbol.LabelLine.Length).Trim();
			return name.Length > 0 ? ToLocalName(name) : AssetName;
		}

		/// <summary>
		/// Builds a single storyboard from the next set of lines in the file.
		/// </summary>
		/// <param name="lines">The remaining lines of data.</param>
		/// <returns>A StoryboardData object.</returns>
		private StoryboardData ProcessStoryboard(Queue<NumberedLine> lines, string name) {
			var panels = new List<PanelInfo>();
			var properties = new Dictionary<string, string>();
			var panelProps = new Dictionary<string, string>();

			var moreLinesInBoard = true;
			while (lines.Count > 0 && moreLinesInBoard) {
				var next = lines.Peek().Text.TrimStart();
				// No more lines in the storyboard
				if (next.StartsWith(Symbol.LabelLine)) moreLinesInBoard = false;
				// Parse a storyboard property
				else if (next.StartsWith(Symbol.BoardProperty)) {
					ParseProperty(lines.Dequeue(), properties);
				}
				// Parse a panel property
				else if (next.StartsWith(Symbol.PanelProperty)) {
					ParseProperty(lines.Dequeue(), panelProps);
				}
				else if (!string.IsNullOrWhiteSpace(next)) {
					PanelInfo panelInfo = null;
					// Parse a narrative panel
					if (next.StartsWith(Symbol.NarrativeLine)) {
						panelInfo = ParseNarrativePanel(lines, panelProps);
					}
					// Parse a decision panel
					else if (next.StartsWith(Symbol.PromptLine)) {
						panelInfo = ParseDecisionPanel(lines, panelProps);
					}
					// Parse an auto-branch
					else if (next.StartsWith(Symbol.ConditionOpen) || next.TrimStart().StartsWith(Symbol.BranchLine)) {
						panelInfo = ParseBranchPanel(lines, panelProps);
					}
					// If a panel was successfully parsed, add it to the list and start a new property set.
					if (panelInfo != null) {
						panels.Add(panelInfo);
						panelProps = new Dictionary<string, string>();
					}
				}
				else _ = lines.Dequeue();
			}

			return new StoryboardData(name, panels, properties);
		}

		/// <summary>
		/// Split a property line into it's key-value pair and place into a given dictionary.
		/// </summary>
		/// <param name="line">The full content of the line to process.</param>
		/// <param name="properties">Dictionary to add the new property to if parse is successful.</param>
		private void ParseProperty(NumberedLine line, Dictionary<string, string> properties) {
			var keyValPair = line.Text.Substring(line.Text.IndexOf(Symbol.PropertyChar) + 1);
			var keyValSplit = keyValPair.Split(Symbol.PropertyDelim.ToCharArray(), 2);

			if (keyValSplit.Length > 1) {
				properties[keyValSplit[0].Trim()] = keyValSplit[1].Trim();
			}
			else {
				WarnInvalidProperty(line);
			}
		}

		/// <summary>
		/// Attempt to create a NarrativePanelInfo from the next set of lines on the queue and
		/// a prepoulated property set.
		/// </summary>
		/// <param name="lines">The available lines to process.</param>
		/// <param name="properties">A set of prepopulated panel properties.</param>
		/// <returns>The info for a single NarrativePanel</returns>
		private NarrativePanelInfo ParseNarrativePanel(Queue<NumberedLine> lines, Dictionary<string, string> properties) {
			// Process lines
			var narrationLines = new List<string>(NarrationLines);
			while (lines.Count > 0
				&& narrationLines.Count < NarrationLines
				&& lines.Peek().Text.StartsWith(Symbol.NarrativeLine)
				) {
				var formattedLine = lines.Dequeue().Text.Substring(1).TrimStart();
				narrationLines.Add(formattedLine);
			}
			var narration = string.Join("\n", narrationLines);

			// Process effects
			var effectBuilder = new StringBuilder();
			var hasEffects = true;
			var lineNumbers = new List<int>();
			// Check for any immediately adjacent effect lines and combine them. Ignore empty lines.
			while (lines.Count > 0 && hasEffects) {
				if (string.IsNullOrWhiteSpace(lines.Peek().Text)) {
					_ = lines.Dequeue();
				}
				else if (lines.Peek().Text.StartsWith(Symbol.EffectLine)) {
					var next = lines.Dequeue();
					_ = effectBuilder.Append(next.Text);
					lineNumbers.Add(next.Number);
				}
				else hasEffects = false;
			}
			var effectLine = effectBuilder.ToString();
			var effect = new NarrativeEffect {
				statChanges = new List<StatChange>(),
				characterTraits = new List<string>(),
				encounterTraits = new List<string>(),
				sessionTraits = new List<string>()
			};
			PopulateStatChanges(effectLine, ref effect, lineNumbers);
			PopulateTraitChanges(effectLine, ref effect, lineNumbers);

			return new NarrativePanelInfo(properties, narration, effect);
		}

		/// <summary>
		/// Populates a given NarrativeEffect struct with all parsable stat changes found in a line.
		/// </summary>
		/// <param name="input">The data line to parse.</param>
		/// <param name="effect">The struct to populate.</param>
		private void PopulateStatChanges(string input, ref NarrativeEffect effect, List<int> lineNumbers) {
			var statMatches = StatRegex.Matches(input);
			foreach (Match match in statMatches) {
				// Check if a matched stat is invalid
				if (!StatMap.ContainsKey(match.Groups[1].Value)) {
					WarnInvalidStat(match.Groups[1].Value, lineNumbers);
					continue;
				}
				// Otherwise, process as normal
				var stat = StatMap[match.Groups[1].Value];
				var operation = match.Groups[2].Value[0];
				_ = int.TryParse(match.Groups[3].Value, out var value);

				switch (operation) {
					case Symbol.AddChar:
						effect.statChanges.Add(new StatChange(stat, value, true));
						break;
					case Symbol.SubChar:
						effect.statChanges.Add(new StatChange(stat, -value, true));
						break;
					case Symbol.SetChar:
						effect.statChanges.Add(new StatChange(stat, value, false));
						break;
				}
			}
		}

		/// <summary>
		/// Populates a given NarrativeEffect struct with all parsable trait changes found in a line.
		/// </summary>
		/// <param name="input">The data line to parse.</param>
		/// <param name="effect">The struct to populate.</param>
		private void PopulateTraitChanges(string input, ref NarrativeEffect effect, List<int> lineNumbers) {
			var traitMatches = TraitRegex.Matches(input);
			foreach (Match match in traitMatches) {
				var target = match.Groups[1].Value;
				var trait = match.Groups[2].Value;

				if (string.IsNullOrWhiteSpace(target)) {
					effect.characterTraits.Add(trait);
				}
				else if (Symbol.Encounter == target.ToLower()) {
					effect.encounterTraits.Add(trait);
				}
				else if (Symbol.Session == target.ToLower()) {
					effect.sessionTraits.Add(trait);
				}
				else WarnInvalidTrait(target, lineNumbers);
			}
		}

		/// <summary>
		/// Attempt to create a DecisionPanelInfo from the next set of lines on the queue and
		/// a prepoulated property set.
		/// </summary>
		/// <param name="lines">The available lines to process.</param>
		/// <param name="properties">A set of prepopulated panel properties.</param>
		/// <returns>The info for a single DecisionPanel</returns>
		private DecisionPanelInfo ParseDecisionPanel(Queue<NumberedLine> lines, Dictionary<string, string> properties) {
			// Parse prompt lines
			var promptLines = new List<string>(PromptLines);
			while (lines.Count > 0 && lines.Peek().Text.StartsWith(Symbol.PromptLine)) {
				if (promptLines.Count < promptLines.Capacity) {
					var formattedLine = lines.Dequeue().Text.Substring(1).TrimStart();
					promptLines.Add(formattedLine);
				}
				else WarnTooManyPromptLines(lines.Peek().Number);
			}
			var prompt = string.Join("\n", promptLines);

			var decisions = new List<DecisionPanelInfo.Decision>();
			var hasMoreDecisions = true;
			while (lines.Count > 0 && hasMoreDecisions) {
				var condition = ParseCondition(lines);
				hasMoreDecisions = TryParseDecisionLine(lines, out var decision);
				if (hasMoreDecisions) {
					var (text, uri, order) = decision;
					decisions.Add(new DecisionPanelInfo.Decision(text, uri, condition, order));
				}
			}

			return new DecisionPanelInfo(properties, prompt, decisions);
		}

		/// <summary>
		/// Combine and format the next set of lines that belong to a single condition.
		/// </summary>
		/// <param name="lines">The queue of remaining lines.</param>
		/// <returns>The parseable condition string, else an empty string.</returns>
		private string ParseCondition(Queue<NumberedLine> lines) {
			while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines.Peek().Text)) _ = lines.Dequeue();
			if (lines.Count < 1) return "";

			var conditionBuilder = new StringBuilder();
			var next = lines.Peek().Text.Trim();
			if (next.StartsWith(Symbol.ConditionOpen)) {
				_ = lines.Dequeue();
				while (lines.Count > 0 && !next.EndsWith(Symbol.ConditionClose)) {
					_ = conditionBuilder.AppendLine(next);
					next = lines.Dequeue().Text.Trim();
				}
				_ = conditionBuilder.Append(next);
			}
			var condition = conditionBuilder.ToString();
			// Strip the enclosing braces and return
			return condition.Length > 2 ? condition.Substring(1, condition.Length - 2) : "";
		}

		/// <summary>
		/// Combine and format the next set of lines that belong to a single decision.
		/// </summary>
		/// <param name="lines">The queue of remaining lines.</param>
		/// <param name="output">A tuple holding the parsed pieces of the decision.</param>
		/// <returns>True if a well formatted decision line was found and parsed, else false.</returns>
		private bool TryParseDecisionLine(Queue<NumberedLine> lines, out (string, string, int) output) {
			output = ("", "", 0);
			while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines.Peek().Text)) _ = lines.Dequeue();
			if (lines.Count < 1) return false;

			var next = lines.Peek().Text.Trim();
			if (next.StartsWith(Symbol.BranchLine)) {
				_ = lines.Dequeue();
				var match = DecisionRegex.Match(next);
				if (match.Success) {
					_ = int.TryParse(match.Groups[1].Value, out var order);
					var text = match.Groups[2].Value.Trim();
					var uri = ProcessUri(match.Groups[3].Value.Trim());
					output = (text, uri, order);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Attempt to create a BranchPanelInfo from the next set of lines on the queue and
		/// a prepoulated property set.
		/// </summary>
		/// <param name="lines">The available lines to process.</param>
		/// <param name="properties">A set of prepopulated panel properties.</param>
		/// <returns>The info for a single BranchPanel</returns>
		private BranchPanelInfo ParseBranchPanel(Queue<NumberedLine> lines, Dictionary<string, string> properties) {
			var branches = new List<BranchPanelInfo.Data>();
			var hasMoreBranches = true;
			while (lines.Count > 0 && hasMoreBranches) {
				var conditionText = ParseCondition(lines);
				hasMoreBranches = TryParseBranchLine(lines, out var branch);
				var (nextPanel, orderNum) = branch;
				branches.Add(new BranchPanelInfo.Data(nextPanel, conditionText, orderNum));
			}
			return new BranchPanelInfo(properties, branches);
		}

		/// <summary>
		/// Combine and format the next set of lines that belong to a single branch.
		/// </summary>
		/// <param name="lines">The queue of remaining lines.</param>
		/// <returns>A tuple containing the next storyboard uri, and the playback order of a branchS.</returns>
		private bool TryParseBranchLine(Queue<NumberedLine> lines, out (string, int) output) {
			output = ("", 0);
			while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines.Peek().Text)) _ = lines.Dequeue();
			if (lines.Count < 1) return false;

			var next = lines.Peek().Text.Trim();
			if (next.StartsWith(Symbol.BranchLine)) {
				_ = lines.Dequeue();
				var match = BranchRegex.Match(next);
				if (match.Success) {
					_ = int.TryParse(match.Groups[1].Value, out var order);
					var uri = ProcessUri(match.Groups[2].Value.Trim());
					output = (uri, order);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Takes in a raw storyboard reference value and performs and performs any needed post processing.
		/// </summary>
		/// <param name="value">The raw storyboard reference value.</param>
		/// <returns>A processed storyboard uri.</returns>
		private string ProcessUri(string value) => value.Contains(":") ? value : ToLocalRef(value);

		/// <summary>
		/// Formats a string into a uri representing a storyboard local to the file.
		/// </summary>
		/// <param name="value">The storyboard reference value.</param>
		/// <returns>The formatted local storyboard uri.</returns>
		private string ToLocalRef(string value) => $"data:{ToLocalName(value)}";

		/// <summary>
		/// Formats a string into a local storyboard name by prepending the containing filename.
		/// </summary>
		/// <param name="value">The raw storyboard name.</param>
		/// <returns>The formatted local storyboard name.</returns>
		private string ToLocalName(string value) => $"{AssetName}-{value}";
	}
}