using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages an individual stat field on the Stat Update Display.
/// </summary>
public class StatUpdateField : MonoBehaviour {
	[SerializeField]
	private CharacterStat stat;
	public CharacterStat Stat => stat;

	[SerializeField]
	private Text numberDisplay;
	[SerializeField]
	private List<Text> decorativeTexts;

	[SerializeField]
	private float fadeTime = 0.8f;

	[SerializeField]
	private Color positiveColor = Color.green;
	[SerializeField]
	private Color negativeColor = Color.red;

	private int statValue = 0;
	public int Value {
		get => statValue;
		set {
			statValue = value;
			numberDisplay.text = value.ToString();
		}
	}
	private Color originalColor;
	public Color Color {
		get => numberDisplay.color;
		set {
			numberDisplay.color = value;
			foreach (var text in decorativeTexts) text.color = value;
		}
	}

	public void Increment() {
		Color = positiveColor;
		Value++;
		timer = 0;
		enabled = true;
	}

	public void Decrement() {
		Color = negativeColor;
		Value--;
		timer = 0;
		enabled = true;
	}

	public void Set(int value) {
		if (value > Value) Color = positiveColor;
		else if (value < Value) Color = negativeColor;
		Value = value;
		timer = 0;
		enabled = true;
	}

	private float timer = 0;

	private void Awake() {
		originalColor = numberDisplay.color;
	}

	private void Update() {
		timer += Time.unscaledDeltaTime;
		var normalizedTime = fadeTime <= 0 ? 1 : timer <= 0 ? 0 : timer / fadeTime;
		Color = Color.Lerp(Color, originalColor, normalizedTime);

		if (timer >= fadeTime) {
			Color = originalColor;
			enabled = false;
		}
	}
}
