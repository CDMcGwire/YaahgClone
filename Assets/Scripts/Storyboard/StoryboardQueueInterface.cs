using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component to attach to objects that need to communicate with the Storyboard Queue.
/// </summary>
public class StoryboardQueueInterface : MonoBehaviour {
	public string Uri = "";
	public Storyboard StoryboardPrefab;
	public bool Interrupt = false;
	public bool Merge = true;

	public void EnqueueBoard() => StoryboardQueue.Enqueue(StoryboardPrefab, Interrupt, Merge);
	public void EnqueueBoard(List<int> owningPlayers) => StoryboardQueue.Enqueue(StoryboardPrefab, owningPlayers, Interrupt, Merge);

	public void EnqueueUri() => StoryboardQueue.Enqueue(Uri, Interrupt, Merge);
	public void EnqueueUri(List<int> owningPlayers) => StoryboardQueue.Enqueue(Uri, owningPlayers, Interrupt, Merge);
}
