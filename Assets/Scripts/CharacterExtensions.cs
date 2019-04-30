public static class CharacterExtensions {
	public static bool IsPunctuation(this char character) {
		return character == '.'
			|| character == ','
			|| character == '?'
			|| character == '!'
			|| character == ':'
			|| character == ';'
			|| character == '-'
			|| character == '_'
			|| character == '"'
			|| character == '('
			|| character == ')'
			|| character == '['
			|| character == ']'
			|| character == '{'
			|| character == '}'
		;
	}
	public static bool IsReadSymbol(this char character) {
		return character == '%'
			|| character == '$'
			|| character == '&'
			|| character == '+'
			|| character == '='
			|| character == '*'
			|| character == '^'
			|| character == '@'
		;
	}
}
