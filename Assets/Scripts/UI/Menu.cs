using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// An animatable event controller for UI elements. Implicitly fires open events on Enable.
/// Closing events fire when the Menu is explicitly Closed via the corresponding interface method.
/// If an Animator component is present, it will assume the proper invocations will be called by
/// Animation Events. Otherwise the events will be fired immediately upon activation and closing.
/// </summary>
public class Menu : MonoBehaviour {
	[SerializeField]
	public UnityEvent OnOpening = new UnityEvent();
	public void InvokeOpening() { OnOpening.Invoke(); }
	[SerializeField]
	public UnityEvent OnOpened = new UnityEvent();
	public virtual void InvokeOpened() { OnOpened.Invoke(); }
	[SerializeField]
	public UnityEvent OnClosing = new UnityEvent();
	public void InvokeClosing() { OnClosing.Invoke(); }
	[SerializeField]
	public UnityEvent OnClosed = new UnityEvent();
	public virtual void InvokeClosed() { OnClosed.Invoke(); Deactivate(); }

	[SerializeField]
	private bool destroyOnClose = false;
	public bool DestroyOnClose { get { return destroyOnClose; } set { destroyOnClose = value; } }

	[SerializeField]
	private string animatorTrigger = "Close";

	private Animator animator;

	private bool shouldCycle = false;


	private void Awake() {
		animator = GetComponent<Animator>();
	}

	private void OnEnable() {
		if (!animator) {
			OnOpening.Invoke();
			OnOpened.Invoke();
		}
	}

	public void Close(float delayTime) {
		Invoke("Close", delayTime);
	}

	public void Close() {
		if (!gameObject.activeInHierarchy) return;

		if (animator) animator.SetTrigger(animatorTrigger);
		else {
			InvokeClosing();
			InvokeClosed();
			Deactivate();
		}
	}

	public void Cycle() {
		shouldCycle = true;
		Close();
		if (!animator) { // If no animator present, immediately "reopen"
			OnOpening.Invoke();
			OnOpened.Invoke();
		}
	}

	protected void Deactivate() {
		if (shouldCycle) {
			shouldCycle = false;
			return;
		}
		if (DestroyOnClose) Destroy(gameObject);
		else gameObject.SetActive(false);
	}
}
