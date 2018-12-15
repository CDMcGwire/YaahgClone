using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerEvent : MonoBehaviour {
	[SerializeField]
	private bool paused = false;
	public bool Paused { get { return paused; } set { paused = value; } }

	[Serializable]
	private struct TimeEvent {
		public float triggerTime;
		public UnityEvent onTrigger;
		public bool resetOnEnable;
	}

	[SerializeField]
	private List<TimeEvent> timeEvents = new List<TimeEvent>();

	private Queue<TimeEvent> eventQueue;
	private Queue<TimeEvent> resetQueue;

	private float currentTime = 0;

	private void OnEnable() {
		Reset();
	}

	private void Update() {
		if (!paused) {
			currentTime += Time.deltaTime;
			
			// Trigger all events that have reached their time, while there are more in the queue
			while (eventQueue.Count > 0 && currentTime > eventQueue.Peek().triggerTime) {
				var timeEvent = eventQueue.Dequeue();
				timeEvent.onTrigger.Invoke();
				// If the event can fire again on Reset, add it to the Reset queue
				if (timeEvent.resetOnEnable) resetQueue.Enqueue(timeEvent);
				// If the queue is empty, pause the timer
				if (eventQueue.Count < 1) paused = true;
			}
		}
	}

	public void Reset(bool startPaused = false) {
		currentTime = 0;
		paused = startPaused;

		if (eventQueue == null && resetQueue == null) {
			// Sort the timeEvents by trigger time; lowest first
			timeEvents.Sort((a, b) => a.triggerTime.CompareTo(b.triggerTime));
			// Perform queue initialization
			eventQueue = new Queue<TimeEvent>(timeEvents);
			resetQueue = new Queue<TimeEvent>();
		}
		else {
			resetQueue.FillFrom(eventQueue); // Transfer unfired events to the reset queue to maintain order
			eventQueue.FillFrom(resetQueue); // Refill event queue from everything that can still fire
		}
	}
}
