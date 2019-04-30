public static class StringExtensions {
	/// <summary>
	/// Splits the given string in two at the last instance of the given delimiter.
	/// </summary>
	/// <param name="value">The string to split.</param>
	/// <param name="seperator">The char value to split on.</param>
	/// <returns>
	/// A string tuple (beginning, end). If the given seperator char is not present
	/// beginning will return as the seperator and end will be the original value.
	/// </returns>
	public static (string, string) SplitOnLast(this string value, char seperator) {
		var splitIndex = value.LastIndexOf(seperator);
		var beginning = splitIndex > -1 ? value.Substring(0, splitIndex) : seperator.ToString();
		var end = splitIndex > -1 ? value.Substring(splitIndex + 1) : value;
		return (beginning, end);
	}
}
