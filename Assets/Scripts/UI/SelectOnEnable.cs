using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectOnEnable : MonoBehaviour {
	[SerializeField]
	private Selectable targetSelectable;

	private GameObject lastSelected;

	private void OnEnable() {
		if (!targetSelectable || !EventSystem.current) return;
		StartCoroutine(SelectOnNextFrame());
	}

	private void Select() {
		lastSelected = EventSystem.current.firstSelectedGameObject;

		EventSystem.current.SetSelectedGameObject(targetSelectable.gameObject, null);
		EventSystem.current.firstSelectedGameObject = targetSelectable.gameObject;
		targetSelectable.Select();
		targetSelectable.OnSelect(null);
	}

	private void OnDisable() {
		if (!EventSystem.current 
			|| !targetSelectable 
			|| EventSystem.current.firstSelectedGameObject != targetSelectable.gameObject
			) return;

		var restoredSelectable = lastSelected ?? FindObjectOfType<Selectable>().gameObject;
		EventSystem.current.SetSelectedGameObject(restoredSelectable, null);
		EventSystem.current.firstSelectedGameObject = restoredSelectable;
	}

	private IEnumerator SelectOnNextFrame() {
		yield return null;
		Select();
	}
}
