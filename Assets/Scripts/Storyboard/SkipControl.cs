using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Allows players to skip past a panel by pressing the specified button.</summary>
public class SkipControl : PanelControl {
	[SerializeField]
	private UnityEvent onSkip ;
	public UnityEvent OnSkip { get { return onSkip; } }

	[SerializeField]
	private SkipIndicator indicatorPrefab;

	[SerializeField]
	private Menu indicatorMenu;

	[SerializeField]
	private Transform indicatorParent;

	[SerializeField]
	private bool controlEnabled = false;
	public bool ControlEnabled { get { return controlEnabled; } set { controlEnabled = value; } }
	
	private Dictionary<string, SkipIndicator> skipIndicators = new Dictionary<string, SkipIndicator>();
	private int activeIndicatorCount = 0;
	private bool skipped = false;

	protected override void Init() {
		// Validate
		if (!indicatorPrefab) Debug.LogError("No prefab set for indicator on skip control");

		if (Panel.Board.OwningPlayers == null) return;
		else if (Panel.Board.OwningPlayers.Count < 1) {
			indicatorMenu.gameObject.SetActive(false);
			enabled = false;
		}
		if (indicatorParent == null) indicatorParent = transform;
		
		// Clear any prexisting indicators
		foreach (Transform child in indicatorParent) Destroy(child);
		skipIndicators.Clear();

		// Instantiate all required indicators; One per unique controller
		foreach (var player in Panel.Board.OwningPlayers) {
			var controllerCode = InputManager.GetPlayerControllerCode(player);

			if (controllerCode != null && !skipIndicators.ContainsKey(controllerCode)) {
				var indicator = Instantiate(indicatorPrefab, indicatorParent);

				skipIndicators[controllerCode] = indicator;
				indicator.gameObject.SetActive(false);
			}
		}
		if (skipIndicators.Count < 1) {
			Debug.LogWarning("Skip control initialized, but no indicators were created. Controller will be disabled.");
			enabled = false;
		}
	}

	public void Hide() {
		foreach (var indicator in skipIndicators.Values) indicator.Close();
		activeIndicatorCount = 0;
	}

	private void Start() {
		if (indicatorMenu == null) Debug.LogError("No indicator menu specified for skip control");
		else indicatorMenu.gameObject.SetActive(false);
	}

	private void Update() {
		if (skipped) return;

		foreach (var player in Panel.Board.OwningPlayers) {
			if (InputManager.GetPlayerButtonDown(player, "Submit")) {
				if (ControlEnabled) CheckSkip(player);
				else indicatorMenu.gameObject.SetActive(true);
			}
		}
		if (activeIndicatorCount >= skipIndicators.Count) {
			onSkip.Invoke();
			skipped = true;
		}
	}

	private void CheckSkip(int player) {
		var playerController = InputManager.GetPlayerControllerCode(player);
		if (playerController == null) return;

		var indicator = skipIndicators[playerController];
		if (indicator == null) return;

		if (!indicator.gameObject.activeSelf) {
			indicator.gameObject.SetActive(true);
			indicator.Color = CharacterManager.GetLivingCharacter(player).Data.Color;
			activeIndicatorCount++;
		}
	}
}
