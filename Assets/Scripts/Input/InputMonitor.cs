using UnityEngine;

public abstract class InputMonitor : MonoBehaviour {
	private void OnEnable() => MonitorManager.TakeControl(this);

	private void OnDisable() => MonitorManager.Release(this);

	public abstract void CheckForInput();
}
