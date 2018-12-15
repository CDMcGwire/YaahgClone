using System.Collections.Generic;
using UnityEngine;

public class Session : MonoBehaviour {
	private static Session current;
	public static Session Current {
		get {
			if (current == null) current = FindObjectOfType<Session>();
			return current;
		}
	}

	[SerializeField]
	private List<string> traits = new List<string>();
	public static List<string> Traits { get { return Current.traits; } }

	public void HandleCharacterSpawned(Character character) {
		character.OnDeath.AddListener((c, limits) => {
			foreach (var stat in limits) AddTrait(DeathTraits.From(stat));
		});
	}

	public static void AddTrait(string trait) {
		if (Traits.Contains(trait)) return;
		Traits.Add(trait);
	}

	public static bool HasTrait(string trait) {
		return Traits.Contains(trait);
	}

	public static void StartNew() {
		Traits.Clear();
	}
}
