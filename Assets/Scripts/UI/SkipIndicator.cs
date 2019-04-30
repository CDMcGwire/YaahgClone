using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SkipIndicator : MonoBehaviour {
	[SerializeField]
	private Image image;

	public Color Color { get => image.color; set => image.color = value; }

	private void OnValidate() {
		if (image == null) { GetComponent<Image>(); }
	}

	internal void Close() {
		gameObject.SetActive(false);
	}
}
