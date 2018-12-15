using UnityEngine;

public abstract class Path : MonoBehaviour {
	public abstract Panel GetNextPanel(GameData gameData);
}
