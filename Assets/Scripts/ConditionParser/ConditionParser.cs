using System;

public class ConditionParser {
	private readonly GameData gameData;

	private TargetToken.Type currentTargetType;
	private Character currentCharacter;

	public ConditionParser(GameData data) {
		gameData = data;
	}

	public bool Evaluate(string script) {
		if (script == null) throw new ParsingException("Tried to parse a null script");

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
				case TargetToken.Type.CHAR:
					hasTrait = currentCharacter.Data.HasTrait(traitPhrase.trait);
					break;
				case TargetToken.Type.STORY:
					hasTrait = gameData.StoryboardTraits.Contains(traitPhrase.trait);
					break;
				case TargetToken.Type.SESS:
					hasTrait = gameData.SessionTraits.Contains(traitPhrase.trait);
					break;
			}

			return traitPhrase.inverse ? !hasTrait : hasTrait;
		}
		else if (phraseExp.phrase is StatPhrase) {
			var statPhrase = phraseExp.phrase as StatPhrase;
			var characterStat = currentCharacter.Data.GetStat(statPhrase.stat);
			var target = statPhrase.number;

			switch (statPhrase.comparison) {
				case ComparisonToken.Type.GT:
					return characterStat > target;
				case ComparisonToken.Type.LT:
					return characterStat < target;
				case ComparisonToken.Type.EQ:
					return characterStat == target;
				case ComparisonToken.Type.NE:
					return characterStat != target;
			}
		}
		return false;
	}

	public class ParsingException : Exception {
		public ParsingException(string message) : base(message) { }
	}
}
