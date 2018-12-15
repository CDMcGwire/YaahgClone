using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Quick and dirty override of Standalone Input Module with touch/mouse processing removed.
/// </summary>
[AddComponentMenu("Event/Controller Input Module")]
public class ControllerInputModule : StandaloneInputModule {
	/// <summary>
	/// Keeps mouse from being updated on module
	/// </summary>
	public override void UpdateModule() {
		return;
	}

	/// <summary>
	/// More or less a copy of the original implementation but without the final touch/mouse check at the end.
	/// </summary>
	public override void Process() {
		if (!eventSystem.isFocused)
			return;

		bool usedEvent = SendUpdateEventToSelectedObject();

		if (eventSystem.sendNavigationEvents) {
			if (!usedEvent)
				usedEvent |= SendMoveEventToSelectedObject();

			if (!usedEvent)
				SendSubmitEventToSelectedObject();
		}
	}
}
