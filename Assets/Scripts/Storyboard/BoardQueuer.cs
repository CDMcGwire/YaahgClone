using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this component to a GameObject to interface with the StoryboardQueue Singleton.
/// </summary>
public class BoardQueuer : MonoBehaviour {
	/// <summary>
	/// Adds a new instantiation of the referenced storyboard to the queue.
	/// </summary>
	/// <param name="players">A list of all players (by number) that should own the new board.</param>
	/// <param name="storyboard">A GameObject with the Storyboard component.</param>
	public void Queue(List<int> players, Storyboard storyboard) {
		StoryboardQueue.Enqueue(storyboard, players);
	}
}
