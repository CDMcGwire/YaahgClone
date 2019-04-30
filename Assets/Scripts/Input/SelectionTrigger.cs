using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Component to give UI Buttons a handler for selection events.
/// </summary>
public class SelectionTrigger : MonoBehaviour, ISelectHandler, IDeselectHandler, IUpdateSelectedHandler {
	public UnityEvent onSelect = new UnityEvent();
	public UnityEvent onDeselect = new UnityEvent();
	public UnityEvent onUpdateSelected = new UnityEvent();

	public void OnSelect(BaseEventData eventData) { onSelect.Invoke(); }
	public void OnDeselect(BaseEventData eventData) { onDeselect.Invoke(); }
	public void OnUpdateSelected(BaseEventData eventData) { onUpdateSelected.Invoke(); }
}
