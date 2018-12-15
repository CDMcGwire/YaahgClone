using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*	The intention is that when an InputMonitor is activated, it will subscribe to the stack.
 *	Every update, the manager will trigger an input check on the top monitor in the stack.
 *	When a monitor is disabled, it should remove itself from the stack if it is the top element.
 * Whether it is on top or not, it will then trigger a clear out of any null or inactive elements.
 * Thus, if an element buried on the stack is deactivated before control is returned, it will be
 * removed when uncovered and control will pass further down the stack.
 */
public class MonitorManager : MonoBehaviour {
	private static MonitorManager instance;
	public static MonitorManager Instance {
		get {
			if (instance == null) instance = FindObjectOfType<MonitorManager>();
			return instance;
		}
	}

	[SerializeField]
	private bool paused = false;
	public static bool Paused { get { return Instance.paused; } set { Instance.paused = value; } }

	private LinkedList<InputMonitor> monitorList = new LinkedList<InputMonitor>();

	public static void TakeControl(InputMonitor monitor) {
		if (!Instance) return;
		if (!Instance.monitorList.Contains(monitor)) Instance.StartCoroutine(Instance.AddOnNextFrame(monitor));
	}

	public static void Release(InputMonitor monitor) {
		if (!Instance) return;
		Instance.StartCoroutine(Instance.RemoveOnNextFrame(monitor));
	}

	private void Update() {
		if (paused) return;
		if (Instance.monitorList.Count > 0) monitorList.First.Value.CheckForInput();
	}

	private IEnumerator AddOnNextFrame(InputMonitor monitor) {
		yield return null;
		monitorList.AddFirst(monitor);
	}

	private IEnumerator RemoveOnNextFrame(InputMonitor monitor) {
		yield return null;
		monitorList.Remove(monitor);
	}
}
