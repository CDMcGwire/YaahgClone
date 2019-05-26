using System.Collections.Generic;

/// <summary>
/// Extension methods to make working with Dictionaries for convenient.
/// </summary>
public static class DictionaryExtensions {
	/// <summary>
	/// Attempts to get the specified value from the given dictionary.
	/// If the value can't be found, it'll return the given default value
	/// instead.
	/// </summary>
	/// <typeparam name="K">The type of an entry's key.</typeparam>
	/// <typeparam name="V">The type of an entry's value.</typeparam>
	/// <param name="dictionary">The dictionary to operate on.</param>
	/// <param name="key">The key to use for lookup.</param>
	/// <param name="defValue">The value to return if no match is found.</param>
	/// <returns>The value paired to the key if found, else the default.</returns>
	public static V GetOrDefault<K, V>(this IDictionary<K, V> dictionary, K key, V defValue) 
		=> dictionary.TryGetValue(key, out var value) ? value : defValue;

	/// <summary>
	/// Attempts to get the specified value from the given dictionary.
	/// If the value can't be found, it'll return the given default value
	/// instead. (Variant for <see cref="IReadOnlyDictionary{TKey, TValue}"/>)
	/// </summary>
	/// <typeparam name="K">The type of an entry's key.</typeparam>
	/// <typeparam name="V">The type of an entry's value.</typeparam>
	/// <param name="dictionary">The dictionary to operate on.</param>
	/// <param name="key">The key to use for lookup.</param>
	/// <param name="defValue">The value to return if no match is found.</param>
	/// <returns>The value paired to the key if found, else the default.</returns>
	public static V GetOrDefault<K, V>(this IReadOnlyDictionary<K, V> dictionary, K key, V defValue)
		=> dictionary.TryGetValue(key, out var value) ? value : defValue;
}
