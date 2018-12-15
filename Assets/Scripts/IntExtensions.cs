public static class IntExtensions {
	public static bool IsLetter(this int value) {
		return char.IsLetter((char)value);
	}

	public static bool IsDigit(this int value) {
		return char.IsDigit((char)value);
	}
}
