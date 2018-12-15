using UnityEngine;
using UnityEngine.Events;

public class PanelEndEvent : UnityEvent<Panel> { }

[RequireComponent(typeof(Menu))]
[RequireComponent(typeof(ConditionalPath))]
public class Panel : MonoBehaviour {
	public Storyboard Board { get; private set; }

	[SerializeField]
	private PanelEndEvent onComplete = new PanelEndEvent();
	public PanelEndEvent OnComplete { get { return onComplete; } private set { onComplete = value; } }

	[SerializeField]
	private Menu menu;
	public Menu Menu { get { return menu; } private set { menu = value; } }

	[SerializeField]
	private Path path;
	public Path Path { get { return path; } private set { path = value; } }

	private void OnValidate() {
		if (menu == null) menu = GetComponent<Menu>();
		if (path == null) path = GetComponent<Path>();
	}

	public void Initialize(Storyboard board) {
		Board = board;
		foreach (var control in GetComponentsInChildren<PanelControl>()) {
			control.Initialize(this);
		}
	}

	public void EndPanel() {
		onComplete.Invoke(path.GetNextPanel(Board.GameData));
	}
}