using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Sprite))]
public class CharacterSelector : MonoBehaviour {
	[SerializeField]
	[HideInInspector]
	private Sprite sprite;

	[SerializeField]
	private float AxisDeadzone = 0.2f;

	[SerializeField]
	[Range(0, 15)]
	private int playerNumber;
	public int PlayerNumber {
		get { return playerNumber; }
		set {
			playerNumber = value < 0 ? 0 : value > 15 ? 15 : value;
			submitButton = InputManager.GetPlayerControllerCode(value) + "_Submit";
			backButton = InputManager.GetPlayerControllerCode(value) + "_Back";
			horizontalAxis = InputManager.GetPlayerControllerCode(value) + "_Horizontal";
			verticalAxis = InputManager.GetPlayerControllerCode(value) + "_Vertical";
		}
	}

	[SerializeField]
	private UnityEvent onSubmit = new UnityEvent();
	public UnityEvent OnSubmit { get { return onSubmit; } }

	[SerializeField]
	private UnityEvent onBack = new UnityEvent();
	public UnityEvent OnBack { get { return onBack; } }

	private string submitButton = "KB_Submit";
	private string backButton = "KB_Back";
	private string horizontalAxis = "KB_Horizontal";
	private string verticalAxis = "KB_Vertical";

	public List<CharacterSelection> Selections { get; set; }
	private int current = 0;
	public CharacterSelection CurrentSelection { get { return Selections[current]; } }

	private bool needsReset = false;

	private void OnValidate() {
		sprite = GetComponent<Sprite>();
	}

	private void Update() {
		UpdateSelection(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis));

		if (Input.GetButtonDown(submitButton)) onSubmit.Invoke();
		else if (Input.GetButtonDown(backButton)) onBack.Invoke();
	}

	private void UpdateSelection(float horizontalInput, float verticalInput) {
		if (needsReset && Mathf.Abs(verticalInput) < AxisDeadzone) needsReset = false;
		else if (!needsReset) {
			if (verticalInput >= AxisDeadzone) current = current == 0 ? Selections.Count : current - 1;
			else if (verticalInput <= -AxisDeadzone) current = current == Selections.Count ? 0 : current + 1;
		}
	}
}
