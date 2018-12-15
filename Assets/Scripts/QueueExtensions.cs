using System.Collections.Generic;

public static class QueueExtensions {
	/// <summary>
	/// Drains the entirety of the target queue into the calling queue.
	/// </summary>
	/// <typeparam name="T">The type of the queue content</typeparam>
	/// <param name="queue">The calling queue, using '.' notation</param>
	/// <param name="other">The queue to drain from</param>
	public static void FillFrom<T>(this Queue<T> queue, Queue<T> other) {
		while (other.Count > 0) queue.Enqueue(other.Dequeue());
	}
}
