using UnityEngine;

/// <summary>
/// Attach to an object to handle InputManager references and allow for UnityEvent integration
/// </summary>
public class InputManagerInterface : MonoBehaviour {
	public void PauseGameInput() => InputManager.Paused = true;
	public void ResumeGameInput() => InputManager.Paused = false;
	public void AddPlayer(string controllerCode) => _ = InputManager.AddPlayer(controllerCode);
	public void RemovePlayer(int playerNumber) => _ = InputManager.RemovePlayer(playerNumber);
	public void RemoveActivePlayer() => _ = InputManager.RemovePlayer(InputManager.ActivePlayer);
	public void SetActivePlayer(int playerNumber) => InputManager.SetActivePlayer(playerNumber);
	public void ClearRoster() => InputManager.ClearRoster();
}
