using UnityEngine;
using UnityEngine.Events;

public class TransitionEvents : StateMachineBehaviour {
	public string State = "Open";

	public UnityEvent OnEnter = new UnityEvent();
	public UnityEvent OnExit = new UnityEvent();

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		Debug.Log("Entering State " + State);
		OnEnter.Invoke();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		Debug.Log("Exiting State " + State);
		OnExit.Invoke();
	}
}
