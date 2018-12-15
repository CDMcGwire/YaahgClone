using UnityEngine;
using UnityEngine.UI;

public class ConditionTester : MonoBehaviour {
	public GameData gameData;

	[Space(30)]
	[TextArea(5, 10)]
	public string script;

	[Space(30)]
	public bool expectedResult = true;
	public Image indicator;

	private ConditionParser parser;

	private void Start() {
		parser = new ConditionParser(gameData);
	}

	public void TestScript() {
		var scriptResult = parser.Evaluate(script);
		if (indicator) indicator.color = scriptResult == expectedResult ? Color.green : Color.red;
	}
}