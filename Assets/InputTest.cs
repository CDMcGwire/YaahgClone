using System.Collections.Generic;
using UnityEngine;

public class InputTest : MonoBehaviour {
	public List<int> players = new List<int>();

	void Update() {
		foreach (var player in players) {
			if (InputManager.GetPlayerButton(player, "Submit")) Debug.Log($"Player {player + 1} has pressed submit");
		}
	}
}
