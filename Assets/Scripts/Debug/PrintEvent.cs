using UnityEngine;

public class PrintEvent : MonoBehaviour {
	public void Print() {
		Debug.Log("PrintEvent fired from " + name + " on frame " + Time.frameCount);
	}

	public void Print(string message) {
		Debug.Log("PrintEvent fired from " + name + " on frame " + Time.frameCount + " with message : " + message);
	}
}
