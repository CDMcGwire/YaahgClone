using System;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalPath : Path {
	[SerializeField]
	private Panel defaultNext;
	[SerializeField]
	private List<PathCondition> pathConditions = new List<PathCondition>();
	
	public override Panel GetNextPanel(GameData gameData) {
		var conditionParser = new ConditionParser(gameData);

		foreach (var condition in pathConditions) {
			var conditionMet = conditionParser.Evaluate(condition.Script);
			if (conditionMet) return condition.Panel;
		}
		return defaultNext;
	}
}

[Serializable]
public struct PathCondition {
	[SerializeField]
	[TextArea(2, 10)]
	private string script;
	public string Script { get { return script; } }
	[SerializeField]
	private Panel panel;
	public Panel Panel { get { return panel; } }
}
