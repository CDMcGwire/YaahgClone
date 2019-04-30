using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerConfirmationWindow : MonoBehaviour {
	[SerializeField]
	private Text textElement;

	[SerializeField]
	private string displayText = "";
	public string DisplayText {
		get => displayText;
		set {
			if (textElement != null) textElement.text = value;
			displayText = value;
		}
	}

	[SerializeField]
	private UnityEvent onSubmit = new UnityEvent();
	public UnityEvent OnSubmit => onSubmit;

	[SerializeField]
	private UnityEvent onBack = new UnityEvent();
	public UnityEvent OnBack => onBack;

	[SerializeField]
	public int PlayerNumber = -1;

	private bool AnySubmit => InputManager.GetButtonDownFromAny("Submit");
	private bool AnyBack => InputManager.GetButtonDownFromAny("Back");
	private bool PlayerSubmit => InputManager.GetPlayerButtonDown(PlayerNumber, "Submit");
	private bool PlayerBack => InputManager.GetPlayerButtonDown(PlayerNumber, "Back");

	private void OnValidate() {
		if (textElement == null) textElement = GetComponentInChildren<Text>();
	}

	private void Update() {
		if (PlayerNumber < 0) {
			if (AnySubmit) onSubmit.Invoke();
			else if (AnyBack) onBack.Invoke();
		}
		else {
			if (PlayerSubmit) onSubmit.Invoke();
			else if (PlayerBack) onBack.Invoke();
		}
	}
}
