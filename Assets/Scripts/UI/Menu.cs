using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// An animatable event controller for UI elements. Implicitly fires open events on Enable.
/// Closing events fire when the Menu is explicitly Closed via the corresponding interface method.
/// If an Animator component is present, it will assume the proper invocations will be called by
/// Animation Events. Otherwise the events will be fired immediately upon activation and closing.
/// </summary>
public class Menu : MonoBehaviour {
	[Tooltip("Event to fire immediately when the Menu begins to open.")]
	[SerializeField]
	public UnityEvent OnOpening = new UnityEvent();

	[Tooltip("Event to fire when the open animation ends. If no animation present, it will fire immediately after OnOpening.")]
	[SerializeField]
	public UnityEvent OnOpened = new UnityEvent();

	[Tooltip("Event to fire immediately when the Menu begins to close.")]
	[SerializeField]
	public UnityEvent OnClosing = new UnityEvent();

	[Tooltip("Event to fire when the close animation ends. If no animation present, it will fire immediately after OnClosing.")]
	[SerializeField]
	public UnityEvent OnClosed = new UnityEvent();

	/// <summary>Invokes the OnOpening event. Interface method for the Unity Animation Event system.</summary>
	public void InvokeOpening() => CurrentState = State.Opening;
	/// <summary>Invokes the OnOpened event. Interface method for the Unity Animation Event system.</summary>
	public void InvokeOpened() => CurrentState = State.Open;
	/// <summary>Invokes the OnClosing event. Interface method for the Unity Animation Event system.</summary>
	public void InvokeClosing() => CurrentState = State.Closing;
	/// <summary>Invokes the OnClosed event. Interface method for the Unity Animation Event system.</summary>
	public void InvokeClosed() => CurrentState = State.Closed;

	[Tooltip("If true, the menu will destroy itself when closed.")]
	[SerializeField]
	private bool destroyOnClose = false;
	public bool DestroyOnClose { get => destroyOnClose; set => destroyOnClose = value; }

	[Space(12)]

	[Tooltip("Set to adjust the speed of the open and close animations.")]
	[SerializeField]
	private float initialAnimationSpeed = 1.5f;
	public float AnimationSpeed {
		get => animator ? animator.GetFloat(animatorSpeedProperty) : 0;
		set { if (animator) animator.SetFloat(animatorSpeedProperty, value); }
	}

	[Tooltip("The name of the animator float parameter that should be used to control playback speed.")]
	[SerializeField]
	private string animatorSpeedProperty = "Speed";

	[Tooltip("The name of the animator trigger parameter that should be used to signal when the close animation should begin.")]
	[SerializeField]
	private string animatorTrigger = "Close";

	/// <summary>Reference to the Menu's animator component.</summary>
	private Animator animator;

	/// <summary>The current state of the menu.</summary>
	private State _currentState = State.Closed;
	public State CurrentState {
		get => _currentState;
		private set {
			_currentState = value;
			switch (_currentState) {
				case State.Opening:
					OnOpening.Invoke();
					break;
				case State.Open:
					OnOpened.Invoke();
					break;
				case State.Closing:
					OnClosing.Invoke();
					break;
				case State.Closed:
					Deactivate();
					OnClosed.Invoke();
					break;
			}
		}
	}

	/// <summary>Tracks whether or not the menu should cycle the next time Close is called.</summary>
	private bool shouldCycle = false;

	private void Awake() => animator = GetComponent<Animator>();

	private void OnEnable() {
		AnimationSpeed = initialAnimationSpeed;
		if (animator == null) {
			// Quickly cycle through opening and opened states, firing both events in order.
			CurrentState = State.Opening;
			CurrentState = State.Open;
		}
	}

	/// <summary>
	/// Open the menu by activating it and letting the implicit OnEnable handle the rest.
	/// Does nothing if not currently closed.
	/// </summary>
	public void Open() {
		gameObject.SetActive(true);
	}

	/// <summary>Trigger the process to close the menu after some time.</summary>
	/// <param name="delayTime">The time in seconds to wait before closing.</param>
	public void Close(float delayTime) {
		if (CurrentState == State.Opening || CurrentState == State.Open) Invoke("Close", delayTime);
	}

	/// <summary>
	/// Trigger the process to close the Menu. 
	/// If no animator present, all steps will execute immediately.
	/// </summary>
	public void Close() {
		if (CurrentState == State.Closing || CurrentState == State.Closed) return;

		if (animator != null) {
			animator.SetTrigger(animatorTrigger);
		}
		else {
			// Quickly cycle through closing and closed states, firing both events in order.
			CurrentState = State.Closing;
			CurrentState = State.Closed;
		}
	}

	/// <summary>
	/// Set the menu to Close and immediately reopen, triggering all events in the process.
	/// The Menu will not disable or be destroyed.
	/// </summary>
	public void Cycle() {
		if (CurrentState == State.Closed && !gameObject.activeSelf) Open();
		else {
			shouldCycle = true;
			if (CurrentState == State.Opening || CurrentState == State.Open) Close();
		}
	}

	/// <summary>Call to skip the currently playing animation. All events should fire as normal.</summary>
	public void SkipAnimation() {
		if (animator != null) animator.Play(0, 0, 0.99f);
	}

	/// <summary>
	/// Final step of the close/cycle process. If cycling, it will reset the flag and return,
	/// allowing the menu to reopen. Otherwise the menu will either disable or be destroyed as configured.
	/// </summary>
	protected void Deactivate() {
		if (CurrentState != State.Closed) {
			Debug.LogWarning($"Trying to Deactivate menu {name} but it had not reached the closed state.");
			return;
		}
		else if (shouldCycle) {
			shouldCycle = false;
			OnEnable(); // Cheap way to keep behaviour consistent without waiting for Unity to Enable and Disable it.
			return;
		}
		else if (DestroyOnClose) Destroy(gameObject);
		else gameObject.SetActive(false);
	}

	/// <summary>Enum to manage the state of the menu.</summary>
	public enum State {
		Opening,
		Open,
		Closing,
		Closed
	}
}
