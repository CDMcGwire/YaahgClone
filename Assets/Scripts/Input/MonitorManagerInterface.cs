using UnityEngine;

public class MonitorManagerInterface : MonoBehaviour {
	public void PauseInputMonitor() => MonitorManager.Paused = true;
	public void ResumeInputMonitor() => MonitorManager.Paused = false;
	public void TakeControl(InputMonitor monitor) => MonitorManager.TakeControl(monitor);
	public void ReleaseControl(InputMonitor monitor) => MonitorManager.Release(monitor);
}
