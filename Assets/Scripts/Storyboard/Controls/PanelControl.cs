using UnityEngine;

public abstract class PanelControl : MonoBehaviour {
	public Panel Panel { get; private set; }

	public void Initialize(Panel panel) {
		if (panel == null) return;
		if (panel.Board == null) return;
		Panel = panel;
		Init();
	}

	// Overriden by child class to specify setup
	protected virtual void Init() { }
}
