using UnityEngine;

public abstract class EncounterPicker : MonoBehaviour {
	/// <summary>Get the next storyboard for the day. Null if none remaining.</summary>
	public abstract Storyboard Next { get; }
}
