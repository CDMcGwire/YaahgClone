using System;
using System.Collections.Generic;

public class ConditionParser {
	private readonly GameData gameData;

	private TargetToken.Type currentTargetType;
	private Character currentCharacter;

	public ConditionParser(GameData data) {
		gameData = data;
	}

	/// <summary>Parse the passed string into a condition and resolve against the saved GameData.</summary>
	/// <param name="script">The condition script to parse.</param>
	/// <returns>
	/// True if the script resolves to true with the current data, else false. 
	/// Null or empty scripts return true as there is no condition.
	/// </returns>
	public bool Evaluate(string script) {
		if (string.IsNullOrWhiteSpace(script)) return true;

		var tokens = ConditionTokenizer.Tokenize(script);
		var ast = new ConditionTree(tokens);
		var value = Parse(ast.root);

		return value;
	}

	private bool Parse(ExpressionNode node) {
		if (node is PhraseExpression) {
			var phraseExp = node as PhraseExpression;
			return EvaluatePhrase(phraseExp);
		}
		else if (node is BoolExpression) {
			var boolExp = node as BoolExpression;
			return EvaluateBool(boolExp);
		}
		else if (node is TargetExpression) {
			var targetExp = node as TargetExpression;
			// For use by PhraseExpressions
			currentTargetType = targetExp.target.value;

			// If the target is a character, evaluate the branch until one character returns true
			if (currentTargetType == TargetToken.Type.CHAR) {
				foreach (var character in gameData.Characters) {
					currentCharacter = character;
					if (Parse(targetExp.expression)) return true;
				}
			}
			else return Parse(targetExp.expression);
		}
		return false;
	}

	private bool EvaluateBool(BoolExpression boolExp) {
		var type = boolExp.boolToken.value;
		switch (type) {
			case BoolToken.Type.AND:
				return Parse(boolExp.lhs) && Parse(boolExp.rhs);
			case BoolToken.Type.OR:
				return Parse(boolExp.lhs) || Parse(boolExp.rhs);
		}
		return false;
	}

	private bool EvaluatePhrase(PhraseExpression phraseExp) {
		if (phraseExp.phrase is TraitPhrase) {
			var traitPhrase = phraseExp.phrase as TraitPhrase;

			var hasTrait = false;
			switch (currentTargetType) {
				case TargetToken.Type.PARTY:
					hasTrait = PartyHasTraits(traitPhrase.traits);
					break;
				case TargetToken.Type.CHAR:
					hasTrait = HasAllTraits(traitPhrase.traits, currentCharacter.Data.Traits);
					break;
				case TargetToken.Type.ENC:
					hasTrait = HasAllTraits(traitPhrase.traits, gameData.EncounterTraits);
					break;
				case TargetToken.Type.SESS:
					hasTrait = HasAllTraits(traitPhrase.traits, gameData.SessionTraits);
					break;
			}

			return traitPhrase.inverse ? !hasTrait : hasTrait;
		}
		else if (phraseExp.phrase is StatPhrase) {
			CheckValidStatTarget(currentTargetType);

			var statPhrase = phraseExp.phrase as StatPhrase;
			// If the current target is the party, the stat value is the sum total for the party.
			// Otherwise, it's the value of the individual target character.
			var statValue = currentTargetType == TargetToken.Type.PARTY
				? GetPartyStatTotal(statPhrase.stat)
				: currentCharacter.Data.GetStat(statPhrase.stat);

			var target = statPhrase.number;

			switch (statPhrase.comparison) {
				case ComparisonToken.Type.GT:
					return statValue > target;
				case ComparisonToken.Type.LT:
					return statValue < target;
				case ComparisonToken.Type.EQ:
					return statValue == target;
				case ComparisonToken.Type.NE:
					return statValue != target;
			}
		}
		return false;
	}

	/// <summary>Checks if the party as a whole owns all of the given traits.</summary>
	/// <param name="traits"></param>
	/// <returns></returns>
	private bool PartyHasTraits(List<string> traits) {
		var setOfOwned = new HashSet<string>();
		foreach (var character in gameData.Characters) {
			foreach (var trait in character.Data.Traits) setOfOwned.Add(trait);
		}
		return HasAllTraits(traits, setOfOwned);
	}

	/// <summary>Checks if the target reference contains all the listed traits.</summary>
	/// <param name="traits">The traits to check for.</param>
	/// <param name="reference">All traits owned by the target.</param>
	/// <returns>True if each trait is on the reference, else false.</returns>
	private bool HasAllTraits(List<string> traits, List<string> reference) {
		return HasAllTraits(traits, new HashSet<string>(reference));
	}

	/// <summary>Checks if the target reference contains all the listed traits.</summary>
	/// <param name="traits">The traits to check for.</param>
	/// <param name="reference">All traits owned by the target.</param>
	/// <returns>True if each trait is on the reference, else false.</returns>
	private bool HasAllTraits(List<string> traits, HashSet<string> reference) {
		foreach (var trait in traits) if (!reference.Contains(trait)) return false;
		return true;
	}

	/// <summary>Gets the sum total value of a given character stat for the entire party.</summary>
	/// <param name="stat">The stat to check for.</param>
	/// <returns>The party's sum total for the given stat.</returns>
	private int GetPartyStatTotal(CharacterStat stat) {
		int total = 0;
		foreach (var character in gameData.Characters)
			total += character.Data.GetStat(stat);
		return total;
	}

	/// <summary>
	/// Validates if the correct target type was given for the stat check. 
	/// Throws an exception if invalid.
	/// </summary>
	/// <param name="targetType">The type of the current target.</param>
	private void CheckValidStatTarget(TargetToken.Type targetType) {
		if (targetType != TargetToken.Type.CHAR || targetType != TargetToken.Type.PARTY) {
			throw new LexingException(
				"Attempted to parse a stat condition with invalid target type " + currentTargetType.ToString()
				+ ". Only Character and Party targets can have stat checks."
			);
		}
	}

	public class ParsingException : Exception {
		public ParsingException(string message) : base(message) { }
	}
}
