using System.IO;

public static class ReaderExtensions {
	/// <summary>
	/// Call to read past the next group of whitespace characters
	/// </summary>
	public static void CycleWhitespace(this StringReader reader) {
		while (char.IsWhiteSpace((char)reader.Peek())) reader.Read();
	}
}
