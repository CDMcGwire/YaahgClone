using UnityEngine;

/// <summary>Allows players to skip past a panel by pressing the specified button.</summary>
public class NarrativeControl : PanelControl {
	[Tooltip("ActionMontior which should drive player input.")]
	[SerializeField]
	private ActionMonitor actionMonitor;

	[Tooltip("Prompt that should appear after some time to tell the player how to continue.")]
	[SerializeField]
	private Menu continueIndicator;

	[Tooltip("Time to wait before showing the input prompt.")]
	[SerializeField]
	private float indicatorDelay = 1.0f;

	private void OnValidate() => Debug.Assert(continueIndicator != null, "No continue indicator set for NarrativeControl on " + name);

	protected override void Init() {
		continueIndicator.gameObject.SetActive(false);
		Invoke("OpenIndicator", indicatorDelay);
		// ActionMonitor will start disabled, and should be enabled by the Menu.OnOpened() event.
		actionMonitor.Players = Panel.Board.OwningPlayers;
		actionMonitor.enabled = false;
	}

	/// <summary>Call to open the player input indicator menu.</summary>
	public void OpenIndicator() => continueIndicator.Open();
}
