using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Variant of Storyboard that populates a set of reusable panels according to a
/// pre-loaded StoryboardData object. StoryboardData should be set by the object
/// responsible for queueing the board.
/// </summary>
[RequireComponent(typeof(CachedAssetLoader))]
public class DataDrivenBoard : Storyboard {
	private const string PropBackdrop = "backdrop";
	private const string PropMerge = "merge";
	private const string PropInterrupt = "interrupt";
	private const string PropPartySplit = "can-split-party";

	/// <summary>The data from which to pull when running the storyboard.</summary>
	public StoryboardData StoryboardData { get; set; }

	[Space(12)]
	[Header("Reusable Panels")]

	[Tooltip("Panel to use for narration.")]
	[SerializeField]
	private NarrativePanel narrativePanel;

	[Tooltip("Panel to use for player decisions.")]
	[SerializeField]
	private DecisionPanel decisionPanel;

	[Tooltip("Panel to use for resolving branches.")]
	[SerializeField]
	private BranchPanel branchPanel;

	[Space(12)]
	[Header("Display Elements")]

	[Tooltip("Image element to use for displaying the backdrop.")]
	[SerializeField]
	private Image backdrop;

	/// <summary>
	/// Reference to the attached CachedAssetLoader component. 
	/// Should be set automatically by the OnValidate method.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	private CachedAssetLoader assetLoader;

	/// <summary>The index of the next panel to pull.</summary>
	private int currentPanel = 0;

	protected override Panel NextPanel() {
		if (StoryboardData == null || currentPanel >= StoryboardData.Panels.Count) return null;
		var panelInfo = StoryboardData.Panels[currentPanel++];
		PopulateCommonProperties(panelInfo);
		return PanelFromInfo(panelInfo);
	}

	protected new void OnValidate() {
		base.OnValidate();
		if (assetLoader == null) {
			assetLoader = GetComponent<CachedAssetLoader>();
			Debug.Assert(assetLoader != null, "No asset loader component given on DataDrivenStoryboard " + name);
		}
	}

	/// <summary>
	/// Additional setup to perform when beginning the storyboard.
	/// </summary>
	protected override void AdditionalSetup() => currentPanel = 0;

	/// <summary>Populate panel-by-panel properties that are shared by all panel types.</summary>
	/// <param name="panelInfo">Data to populate from.</param>
	private void PopulateCommonProperties(PanelInfo panelInfo) {
		if (panelInfo.Properties.ContainsKey(PropBackdrop)) SetBackdrop(panelInfo.Properties[PropBackdrop]);
	}

	/// <summary>Determine the panel to use and populate it for playback.</summary>
	/// <param name="panelInfo">Data to populate from.</param>
	/// <returns>Populated panel.</returns>
	private Panel PanelFromInfo(PanelInfo panelInfo) {
		switch (panelInfo.Type) {
			case PanelType.Narrative:
				PopulateNarrativePanel(panelInfo as NarrativePanelInfo);
				return narrativePanel;
			case PanelType.Decision:
				PopulateDecisionPanel(panelInfo as DecisionPanelInfo);
				return decisionPanel;
			case PanelType.Branch:
				PopulateBranchPanel(panelInfo as BranchPanelInfo);
				return branchPanel;
			default:
				return null;
		}
	}

	/// <summary>Populate the narrative panel with the given data object.</summary>
	/// <param name="panelInfo">Data needed for a narrative panel.</param>
	private void PopulateNarrativePanel(NarrativePanelInfo panelInfo) {
		narrativePanel.TextDisplay.text = panelInfo.Text;
		narrativePanel.ProgressionControl.Set(panelInfo.Effect);
	}

	/// <summary>Populate the decision panel with the given data object.</summary>
	/// <param name="panelInfo">Data needed for a decision panel.</param>
	private void PopulateDecisionPanel(DecisionPanelInfo panelInfo) {
		// Get relevant properties
		var mergeProp = panelInfo.Properties.ContainsKey(PropMerge) ? panelInfo.Properties[PropMerge] : "true";
		_ = bool.TryParse(mergeProp, out bool merge);
		var interruptProp = panelInfo.Properties.ContainsKey(PropInterrupt) ? panelInfo.Properties[PropInterrupt] : "false";
		_ = bool.TryParse(interruptProp, out bool interrupt);

		// Clear out existing options.
		decisionPanel.Control.Decisions.Clear();
		foreach (var decisionData in panelInfo.Decisions) {
			// Create a concrete panel decision from the given data.
			var branch = new Branch (
				decisionData.order,
				players => StoryboardQueue.Enqueue(decisionData.next, players, interrupt, merge)
			);
			var panelDecision = new DecisionControlData {
				OptionText = decisionData.text,
				Condition = decisionData.condition,
				Data = branch
			};
			// Add to the decision list.
			decisionPanel.Control.Decisions.Add(panelDecision);
		}
	}

	/// <summary>Populate the narrative panel with the given data object.</summary>
	/// <param name="panelInfo">Data needed for a branch panel.</param>
	private void PopulateBranchPanel(BranchPanelInfo panelInfo) {
		// Get relevant properties
		var mergeProp = panelInfo.Properties.ContainsKey(PropMerge) ? panelInfo.Properties[PropMerge] : "true";
		_ = bool.TryParse(mergeProp, out bool merge);
		var interruptProp = panelInfo.Properties.ContainsKey(PropInterrupt) ? panelInfo.Properties[PropInterrupt] : "false";
		_ = bool.TryParse(interruptProp, out bool interrupt);
		var splitProp = panelInfo.Properties.ContainsKey(PropPartySplit) ? panelInfo.Properties[PropPartySplit] : "true";
		_ = bool.TryParse(splitProp, out bool canSplitParty);

		var conditionalBranches = new List<ConditionalBranch>();
		foreach (var branchData in panelInfo.Branches) {
			var branch = new Branch {
				PlaybackOrder = branchData.playbackOrder
			};
			branch.OnBranch.AddListener(players => StoryboardQueue.Enqueue(branchData.next, players, interrupt, merge));
			conditionalBranches.Add(new ConditionalBranch {
				Condition = branchData.condition,
				Data = branch
			});
		}

		branchPanel.BranchControl.CanSplitParty = canSplitParty;
		branchPanel.BranchControl.ConditionalBranches = conditionalBranches;
	}

	/// <summary>Load the speficied image from an AssetBundle and set the backdrop spite to the loaded asset.</summary>
	/// <param name="imagePath">The URI for the image asset to display.</param>
	private void SetBackdrop(string imagePath) {
		var image = assetLoader.Load<Sprite>(imagePath);
		backdrop.sprite = image;
	}

	/// <summary>DataDriven Storyboard only needs to return a list of the reusable panels.</summary>
	protected override List<Panel> Panels => new List<Panel> { narrativePanel, decisionPanel, branchPanel };
}
