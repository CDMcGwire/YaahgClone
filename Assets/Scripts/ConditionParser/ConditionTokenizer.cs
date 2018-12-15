using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

public static class ConditionTokenizer {
	public static readonly ReadOnlyDictionary<string, Token> KeywordTable = new ReadOnlyDictionary<string, Token>(
		new Dictionary<string, Token>() {
			{ "and", new BoolToken(BoolToken.Type.AND) },
			{ "or", new BoolToken(BoolToken.Type.OR) },
			{ "character", new TargetToken(TargetToken.Type.CHAR) },
			{ "storyboard", new TargetToken(TargetToken.Type.STORY) },
			{ "session", new TargetToken(TargetToken.Type.SESS) },
			{ "has", new HasToken() },
			{ "not", new NotToken() },
			{ "trait", new TraitToken() },
			{ "strength", new StatToken(CharacterStat.STR) },
			{ "str", new StatToken(CharacterStat.STR) },
			{ "toughness", new StatToken(CharacterStat.TUF) },
			{ "tuf", new StatToken(CharacterStat.TUF) },
			{ "dexterity", new StatToken(CharacterStat.DEX) },
			{ "dex", new StatToken(CharacterStat.DEX) },
			{ "perception", new StatToken(CharacterStat.PER) },
			{ "per", new StatToken(CharacterStat.PER) },
			{ "magic", new StatToken(CharacterStat.MAG) },
			{ "mag", new StatToken(CharacterStat.MAG) },
			{ "knowledge", new StatToken(CharacterStat.KNO) },
			{ "kno", new StatToken(CharacterStat.KNO) },
			{ "willpower", new StatToken(CharacterStat.WIL) },
			{ "wil", new StatToken(CharacterStat.WIL) },
			{ "charisma", new StatToken(CharacterStat.CHA) },
			{ "cha", new StatToken(CharacterStat.CHA) },
			{ "swagger", new StatToken(CharacterStat.CHA) },
			{ "madlad", new StatToken(CharacterStat.CHA) },
			{ "lethality", new StatToken(CharacterStat.LET) },
			{ "let", new StatToken(CharacterStat.LET) },
			{ "money", new StatToken(CharacterStat.MONEY) },
			{ "dough", new StatToken(CharacterStat.MONEY) }
		}
	);

	public static List<Token> Tokenize(string input) {
		var initialTokens = ProcessInputString(input);
		var finalTokens = CreateMetaTokens(initialTokens);
		return finalTokens;
	}

	private static Queue<Token> ProcessInputString(string input) {
		var reader = new StringReader(input);
		var tokens = new Queue<Token>();

		while (reader.Peek() != -1) {
			reader.CycleWhitespace(); // Discard whitespace

			if (reader.Peek() == -1) break; // Nothing left to process

			var next = (char)reader.Peek();

			if (next.IsDigit() || next == '-') {
				tokens.Enqueue(ParseNumber(reader));
			}
			else if (next.IsLetter()) {
				tokens.Enqueue(ParseKeyword(reader));
			}
			else {
				switch (next) {
					case '"':
						tokens.Enqueue(ParseString(reader));
						break;
					case '(':
						tokens.Enqueue(new OpenParenthesisToken());
						break;
					case ')':
						tokens.Enqueue(new CloseParenthesisToken());
						break;
					case ',':
						tokens.Enqueue(new CommaToken());
						break;
					case '>':
						tokens.Enqueue(new ComparisonToken(ComparisonToken.Type.GT));
						break;
					case '<':
						tokens.Enqueue(new ComparisonToken(ComparisonToken.Type.LT));
						break;
					case '=':
						tokens.Enqueue(new ComparisonToken(ComparisonToken.Type.EQ));
						break;
					case '!':
						reader.Read(); // Discard '!'
						if ((char)reader.Peek() == '=') tokens.Enqueue(new ComparisonToken(ComparisonToken.Type.NE));
						else throw new InvalidCharacterException("Attempted to parse a \"not equal\" token, but the character following '!' was not an equal sign");
						break;
					default:
						throw new InvalidCharacterException(next);
				}
				reader.Read();
			}
		}

		reader.Close();
		return tokens;
	}

	private static Token ParseString(StringReader reader) {
		reader.Read(); // Discard opening quote

		var builder = new StringBuilder();
		while (reader.Peek() != -1 && reader.Peek() != '"') {
			builder.Append((char)reader.Read());
		}

		return new StringToken(builder.ToString());
	}

	private static Token ParseNumber(StringReader reader) {
		var digits = new StringBuilder();

		// Add minus sign if present
		if ((char)reader.Peek() == '-') digits.Append((char)reader.Read());
		// Append all following number characters
		while (reader.Peek().IsDigit()) digits.Append((char)reader.Read());

		var value = int.Parse(digits.ToString());
		return new NumberToken(value);
	}

	private static Token ParseKeyword(StringReader reader) {
		var builder = new StringBuilder();

		while (IsValidKeywordChar((char)reader.Peek())) builder.Append((char)reader.Read());
		var keyword = builder.ToString().ToLower();

		// Replace with a hash table
		if (KeywordTable.ContainsKey(keyword)) return KeywordTable[keyword];
		else throw new LexingException("Unknown keyword \"" + keyword + "\" found in script");
	}

	private static bool IsValidKeywordChar(char character) {
		return character.IsLetter() || character == '-' || character == '_';
	}

	private static List<Token> CreateMetaTokens(Queue<Token> tokens) {
		var output = new List<Token>();

		while (tokens.Count > 0) {
			var next = tokens.Dequeue();

			if (next is StatToken) {
				var comparison = tokens.Dequeue();
				if (!(comparison is ComparisonToken)) throw new LexingException("Found a stat comparison phrase, but it was not proceeded by a comparison operator");
				var targetValue = tokens.Dequeue();
				if (!(targetValue is NumberToken)) throw new LexingException("Found a stat comparison phrase, but it was missing a number after the comparison operator");

				output.Add(new StatPhrase(next as StatToken, comparison as ComparisonToken, targetValue as NumberToken));
			}
			else if (next is HasToken) {
				next = tokens.Dequeue();

				bool inverse = next is NotToken;
				if (inverse) next = tokens.Dequeue();

				if (!(next is TraitToken)) throw new LexingException("Found a trait phrase, but it did not contain the trait phrase after a 'has' or 'not'");
				next = tokens.Dequeue();
				if (!(next is StringToken)) throw new LexingException("Found a trait phrase, but it did not end with a string value");

				output.Add(new TraitPhrase(inverse, next as StringToken));
			}
			else {
				output.Add(next);
			}
		}

		return output;
	}
}

public class InvalidCharacterException : Exception {
	public InvalidCharacterException(char character)
		: base("An invalid character '" + character + "' was encountered while parsing") { }
	public InvalidCharacterException(string message)
		: base(message) { }
}

public class LexingException : Exception {
	public LexingException(string message) : base(message) { }
}

#region Token Definitions
public abstract class Token { }

public class StringToken : Token {
	public readonly string value;
	public StringToken(string value) { this.value = value; }
}
public class NumberToken : Token {
	public readonly int value;
	public NumberToken(int value) { this.value = value; }
}
public class StatToken : Token {
	public readonly CharacterStat value;
	public StatToken(CharacterStat stat) { value = stat; }
}
public class BoolToken : Token {
	public enum Type { AND, OR }
	public readonly Type value;
	public BoolToken(Type type) { value = type; }
}
public class ComparisonToken : Token {
	public enum Type { GT, LT, EQ, NE }
	public readonly Type value;
	public ComparisonToken(Type type) { value = type; }
}
public class TargetToken : Token {
	public enum Type { CHAR, STORY, SESS };
	public readonly Type value;
	public TargetToken(Type type) { value = type; }
}
public class OpenParenthesisToken : Token { }
public class CloseParenthesisToken : Token { }
public class CommaToken : Token { }
public class HasToken : Token { }
public class NotToken : Token { }
public class TraitToken : Token { }

// Phrases (Meta tokens)
public abstract class Phrase : Token { }
public class StatPhrase : Phrase {
	public readonly CharacterStat stat;
	public readonly ComparisonToken.Type comparison;
	public readonly int number;

	public StatPhrase(StatToken stat, ComparisonToken comparison, NumberToken number) {
		this.stat = stat.value;
		this.comparison = comparison.value;
		this.number = number.value;
	}
}
public class TraitPhrase : Phrase {
	public readonly bool inverse;
	public readonly string trait;

	public TraitPhrase(bool inverse, StringToken trait) {
		this.inverse = inverse;
		this.trait = trait.value;
	}
}
#endregion Token Definitions
