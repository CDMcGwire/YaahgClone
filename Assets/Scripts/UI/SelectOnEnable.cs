using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectOnEnable : MonoBehaviour {
	[SerializeField]
	public Selectable TargetSelectable;

	private GameObject lastSelected;

	private void OnEnable() {
		if (!TargetSelectable || !EventSystem.current) return;
		_ = StartCoroutine(SelectOnNextFrame());
	}

	private void Select() {
		lastSelected = EventSystem.current.firstSelectedGameObject;

		EventSystem.current.SetSelectedGameObject(TargetSelectable.gameObject, null);
		EventSystem.current.firstSelectedGameObject = TargetSelectable.gameObject;
		TargetSelectable.Select();
		TargetSelectable.OnSelect(null);
	}

	private void OnDisable() {
		if (!EventSystem.current 
			|| !TargetSelectable 
			|| EventSystem.current.firstSelectedGameObject != TargetSelectable.gameObject
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
