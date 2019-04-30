using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Menu))]
public class Panel : MonoBehaviour {
	/// <summary>Signals to the storyboard whether or not the owner information display should be enabled while active.</summary>
	[SerializeField]
	private bool showOwnerInfo = true;
	public bool ShowOwnerInfo => showOwnerInfo;

	[SerializeField]
	private UnityEvent onComplete = new UnityEvent();
	public UnityEvent OnEnd { get => onComplete; private set => onComplete = value; }

	[SerializeField]
	private Menu menu;
	public Menu Menu { get => menu; private set => menu = value; }

	public Storyboard Board { get; private set; }

	/// <summary>
	/// Tracks if the panel will end when next closed. Used to accomodate for panels
	/// that cycle to refresh their contents.
	/// </summary>
	private bool ending = false;

	/// <summary>
	/// Tracks if the panel has already run once. For use in one-time setup tasks.
	/// </summary>
	private bool alreadyRun = false;

	private void OnValidate() {
		if (Menu == null) Menu = GetComponent<Menu>();
	}

	/// <summary>Prepare the panel for playback.</summary>
	/// <param name="board">Reference to the calling storyboard.</param>
	public void Setup(Storyboard board) {
		if (!alreadyRun) {
			OneTimeSetup();
			alreadyRun = true;
		}

		Board = board;
		foreach (var control in GetComponentsInChildren<PanelControl>()) {
			control.Initialize(this);
		}
		// Setup the owner info menu based on the players that control this panel, or close if disabled
		if (board.OwnerInfoMenu != null) {
			if (ShowOwnerInfo) {
				board.OwnerInfoMenu.Initialize(Board.OwningPlayers);
				board.OwnerInfoMenu.Menu.Open();
			}
			else board.OwnerInfoMenu.Menu.Close();
		}
	}

	/// <summary>
	/// Method run the first time the panel is setup to handle tasks that
	/// should only be done even if the panel is reused.
	/// </summary>
	protected virtual void OneTimeSetup() {
		Menu.OnClosed.AddListener(() => {
			if (ending) OnEnd.Invoke();
		});
	}

	/// <summary>Begin playback of the panel.</summary>
	public void Play() {
		ending = false;
		Menu.Open();
	}

	/// <summary>Sets the menu to complete on close and initiates the closure.</summary>
	public void End() {
		if (!ending) {
			ending = true;
			Menu.Close();
		}
	}
}