using UnityEngine;
using UnityEngine.UI;

public class NarrativePanel : Panel {
	[Tooltip("Text Element for displaying the narration.")]
	[SerializeField]
	private Text textDisplay;
	public Text TextDisplay => textDisplay;

	[Tooltip("Control for handling narrative playback. Player list will be set by the panel, but controls should be pre-configured.")]
	[SerializeField]
	private NarrativeControl narrativeControl;
	public NarrativeControl NarrativeControl => narrativeControl;

	[Tooltip("Control for handling changes to player characters or game state.")]
	[SerializeField]
	private ProgressionControl progressionControl;
	public ProgressionControl ProgressionControl => progressionControl;
}
