using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Fires off events at given time intervals.
/// </summary>
public class TimerEvent : MonoBehaviour {
	[Tooltip("Whether or not to calculate based on actual time passed, rather than scaled game time.")]
	[SerializeField]
	private bool unscaledTime = true;

	[Tooltip("Whether or not to allow events to fire when game time is <= 0. Will fire off events retroactively if using realtime.")]
	[SerializeField]
	private bool ignoreGametime = false;

	[Tooltip("Used to pause this timer individually.")]
	[SerializeField]
	private bool paused = false;
	public bool Paused { get => paused; set => paused = value; }

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
			currentTime += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

			// If ignoring Gametime pause or Gametime isn't even paused.
			if (ignoreGametime || !Playback.Paused) {
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

	/// <summary>Fires of all remaining events immediately and pauses.</summary>
	public void ShortCircuit() {
		while (eventQueue.Count > 0) {
			var next = eventQueue.Dequeue();
			next.onTrigger.Invoke();
			if (next.resetOnEnable) resetQueue.Enqueue(next);
		}
		paused = true;
	}
}
