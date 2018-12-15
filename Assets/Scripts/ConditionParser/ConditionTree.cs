using System.Collections.Generic;

public class ConditionTree {
	public readonly ExpressionNode root;

	public ConditionTree(List<Token> tokens) {
		var output = new Queue<Token>();
		var operators = new Stack<Token>();

		foreach (var token in tokens) {
			if (token is TraitPhrase || token is StatPhrase) output.Enqueue(token);
			else if (token is TargetToken || token is OpenParenthesisToken) operators.Push(token);
			else if (token is BoolToken) {
				while (operators.Count > 0 && ShouldUnstack(token as BoolToken, operators.Peek())) {
					output.Enqueue(operators.Pop());
				}
				operators.Push(token);
			}
			else if (token is CommaToken) {
				while (operators.Count > 0 && !(operators.Peek() is TargetToken)) {
					output.Enqueue(operators.Pop());
				}
				if (operators.Count <= 0) throw new LexingException("The condition contained a comma without a matching Target");
				output.Enqueue(operators.Pop()); // Enqueue the Target to output
			}
			else if (token is CloseParenthesisToken) {
				while (operators.Count > 0 && !(operators.Peek() is OpenParenthesisToken)) {
					output.Enqueue(operators.Pop());
				}
				if (operators.Count <= 0) throw new LexingException("The condition contained a mismatched closing parenthesis");
				operators.Pop(); // Throw out the open parenthesis
			}
		}
		while (operators.Count > 0) output.Enqueue(operators.Pop());

		var expressionStack = new Stack<ExpressionNode>();
		while (output.Count > 0) {
			var next = output.Dequeue();

			if (next is Phrase)
				expressionStack.Push(new PhraseExpression(next as Phrase));
			else if (next is BoolToken)
				expressionStack.Push(new BoolExpression(next as BoolToken, expressionStack.Pop(), expressionStack.Pop()));
			else if (next is TargetToken)
				expressionStack.Push(new TargetExpression(next as TargetToken, expressionStack.Pop()));
		}

		root = expressionStack.Pop();
	}

	private bool ShouldUnstack(BoolToken boolToken, Token topOperator) {
		if (topOperator == null
			|| topOperator is TargetToken
			|| topOperator is OpenParenthesisToken
			) return false;
		if (!(topOperator is BoolToken)) throw new LexingException("Somehow an invalid Token type ended up on the operators stack. Good job Connor.");

		// Not bothering with an actual precedence as there are only two operators
		return !(boolToken.value == BoolToken.Type.AND && (topOperator as BoolToken).value == BoolToken.Type.OR);
	}
}

public abstract class ExpressionNode { }

public class PhraseExpression : ExpressionNode {
	public readonly Phrase phrase;

	public PhraseExpression(Phrase phrase) {
		this.phrase = phrase;
	}
}
public class BoolExpression : ExpressionNode {
	public readonly BoolToken boolToken;
	public readonly ExpressionNode lhs;
	public readonly ExpressionNode rhs;

	public BoolExpression(BoolToken boolToken, ExpressionNode lhs, ExpressionNode rhs) {
		this.boolToken = boolToken;
		this.lhs = lhs;
		this.rhs = rhs;
	}
}
public class TargetExpression : ExpressionNode {
	public readonly TargetToken target;
	public readonly ExpressionNode expression;

	public TargetExpression(TargetToken target, ExpressionNode expression) {
		this.target = target;
		this.expression = expression;
	}
}