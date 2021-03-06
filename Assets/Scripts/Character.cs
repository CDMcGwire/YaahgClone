﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class StatChangeEvent : UnityEvent<CharacterState, List<StatChange>> { }

[Serializable] // Limits Reached is a derivitive made to determine death status, thus to avoid reallocating, pass along the list.
public class DeathEvent : UnityEvent<Character, List<CharacterStat>> { }

[Serializable]
public class Character : MonoBehaviour {
	[SerializeField]
	private int playerNumber = -1;
	public int PlayerNumber { get => playerNumber; set => playerNumber = value; }

	[SerializeField]
	private CharacterData characterData;
	public CharacterData Data { get => characterData; set => characterData = value; }

	[SerializeField]
	private StatChangeEvent onStatChange = new StatChangeEvent();
	public StatChangeEvent OnStatChange => onStatChange;

	[SerializeField]
	private DeathEvent onDeath = new DeathEvent();
	public DeathEvent OnDeath => onDeath;

	/// <summary>Indicator of whether or not this character has officially "spawned".</summary>
	public bool Spawned { get; private set; } = false;
	public void SetSpawned() => Spawned = true;

	public void ChangeStat(CharacterStat stat, int value) => ChangeStat(new StatChange(stat, value));

	public void ChangeStat(StatChange statChange) => ChangeStats(new List<StatChange> { statChange });

	public void ChangeStats(List<StatChange> statChanges) {
		var originalState = Data.State;
		foreach (var statChange in statChanges) Data.ChangeStat(statChange);
		OnStatChange.Invoke(originalState, statChanges);

		var limits = Data.GetLimitsReached();
		if (limits.Count > 0) {
			Data.Alive = false;
			OnDeath.Invoke(this, limits);
		}
	}

	public void Kill() {
		Data.Alive = false;
		Session.AddTrait("Player Died");
		OnDeath.Invoke(this, Data.GetLimitsReached());
	}

	public void OnValidate() => name = Data.Name;

	public void Start() => Debug.Assert(playerNumber > -1, "Character has been initialized without an assigned player.");
}

[Serializable]
public class CharacterData {
	[SerializeField]
	private CharacterState state;
	public CharacterState State => state;

	[SerializeField]
	private List<string> traits = new List<string>(); // Unity Inspector has horrible hash set support
	public List<string> Traits => traits;

	/// <summary>Adds the trait to the character, if not already present.</summary>
	/// <param name="trait">The trait identifier.</param>
	public void AddTrait(string trait) {
		if (!traits.Contains(trait)) traits.Add(trait);
	}

	public string Name {
		get => state.name;
		set {
			if (string.IsNullOrWhiteSpace(value)) {
				state.lastname = state.firstname = state.name = "";
				return;
			}

			state.name = value.Trim();

			var firstSpace = state.name.IndexOf(' ');
			if (firstSpace <= 0) {
				state.firstname = state.name;
				state.lastname = "";
			}
			else {
				state.firstname = state.name.Substring(0, firstSpace);
				state.lastname = state.name.Substring(firstSpace + 1, state.name.Length - (firstSpace + 1));
			}
		}
	}
	public string Firstname {
		get {
			if (string.IsNullOrWhiteSpace(state.firstname)) Name = state.name;
			return state.firstname;
		}
	}
	public string Lastname {
		get {
			if (string.IsNullOrWhiteSpace(state.lastname)) Name = state.name;
			return state.lastname;
		}
	}

	public string Title { get => state.title ?? ""; set => state.title = value; }
	public Color Color { get => state.color; set => state.color = value; }

	public int Strength { get => state.strength; set => state.strength = value; }
	public int Toughness { get => state.toughness; set => state.toughness = value; }
	public int Dexterity { get => state.dexterity; set => state.dexterity = value; }
	public int Perception { get => state.perception; set => state.perception = value; }
	public int Magic { get => state.magic; set => state.magic = value; }
	public int Knowledge { get => state.knowledge; set => state.knowledge = value; }
	public int Charisma { get => state.charisma; set => state.charisma = value; }
	public int Willpower { get => state.willpower; set => state.willpower = value; }
	public int Lethality { get => state.lethality; set => state.lethality = value; }

	public int Money { get => state.money; set => state.money = value; }

	public int GetStat(CharacterStat stat) => State.GetStat(stat);
	public void SetStat(CharacterStat stat, int value) => state.SetStat(stat, value);
	public void ChangeStat(StatChange statChange) => state.ChangeStat(statChange);

	public bool Alive { get => state.alive; set => state.alive = value; }

	public int PhysicalThreshold { get => state.physicalThreshold; set => state.physicalThreshold = value; }
	public int MentalThreshold { get => state.mentalThreshold; set => state.mentalThreshold = value; }
	public int DebtThreshold { get => state.debtThreshold; set => state.debtThreshold = value; }

	public bool GetLimitReached(CharacterStat stat) => state.GetLimitReached(stat);

	public List<CharacterStat> GetLimitsReached() => state.GetLimitsReached();

	public int Initiative => Dexterity + Perception;

	public CharacterData() { }

	public CharacterData(CharacterData other) {
		if (other != null) state = other.state;
		traits = new List<string>(other.traits);
	}
}

[Serializable]
public enum CharacterStat {
	STR,
	TUF,
	DEX,
	PER,
	MAG,
	KNO,
	CHA,
	WIL,
	LET,
	MONEY
}

[Serializable]
public struct CharacterState {
	public string name;
	[HideInInspector]
	public string firstname;
	[HideInInspector]
	public string lastname;

	public string title;
	public Color color;

	public int strength;
	public int toughness;
	public int dexterity;
	public int perception;
	public int magic;
	public int knowledge;
	public int charisma;
	public int willpower;
	public int lethality;
	public int money;

	public int GetStat(CharacterStat stat) {
		switch (stat) {
			case CharacterStat.STR:
				return strength;
			case CharacterStat.TUF:
				return toughness;
			case CharacterStat.DEX:
				return dexterity;
			case CharacterStat.PER:
				return perception;
			case CharacterStat.MAG:
				return magic;
			case CharacterStat.KNO:
				return knowledge;
			case CharacterStat.CHA:
				return charisma;
			case CharacterStat.WIL:
				return willpower;
			case CharacterStat.LET:
				return lethality;
			case CharacterStat.MONEY:
				return money;
		}
		return -100;
	}
	public void SetStat(CharacterStat stat, int value) {
		switch (stat) {
			case CharacterStat.STR:
				strength = value;
				break;
			case CharacterStat.TUF:
				toughness = value;
				break;
			case CharacterStat.DEX:
				dexterity = value;
				break;
			case CharacterStat.PER:
				perception = value;
				break;
			case CharacterStat.MAG:
				magic = value;
				break;
			case CharacterStat.KNO:
				knowledge = value;
				break;
			case CharacterStat.CHA:
				charisma = value;
				break;
			case CharacterStat.WIL:
				willpower = value;
				break;
			case CharacterStat.LET:
				lethality = value;
				break;
			case CharacterStat.MONEY:
				money = value;
				break;
		}
	}
	/// <summary>
	/// Applies the stat change to the character. If additive, the stat change is treated as a delta.
	/// Otherwise, the value will overwrite the existing.
	/// </summary>
	/// <param name="statChange">Struct representing the change to make.</param>
	public void ChangeStat(StatChange statChange) {
		if (statChange.additive) {
			var finalValue = GetStat(statChange.stat) + statChange.value;
			SetStat(statChange.stat, finalValue);
		}
		else SetStat(statChange.stat, statChange.value);
	}

	public bool alive;
	
	public int physicalThreshold;
	public int mentalThreshold;
	public int debtThreshold;

	public bool GetLimitReached(CharacterStat stat) {
		var statValue = GetStat(stat);

		switch (stat) {
			case CharacterStat.STR:
			case CharacterStat.TUF:
			case CharacterStat.DEX:
			case CharacterStat.PER:
				return statValue <= physicalThreshold;
			case CharacterStat.MAG:
			case CharacterStat.KNO:
			case CharacterStat.CHA:
			case CharacterStat.WIL:
				return statValue <= mentalThreshold;
			case CharacterStat.MONEY:
				return statValue <= debtThreshold;
			default: return false;
		}
	}

	public List<CharacterStat> GetLimitsReached() {
		var statsPassedLimit = new List<CharacterStat>();
		if (strength <= physicalThreshold) statsPassedLimit.Add(CharacterStat.STR);
		if (toughness <= physicalThreshold) statsPassedLimit.Add(CharacterStat.TUF);
		if (dexterity <= physicalThreshold) statsPassedLimit.Add(CharacterStat.DEX);
		if (perception <= physicalThreshold) statsPassedLimit.Add(CharacterStat.PER);
		if (magic <= mentalThreshold) statsPassedLimit.Add(CharacterStat.MAG);
		if (knowledge <= mentalThreshold) statsPassedLimit.Add(CharacterStat.KNO);
		if (charisma <= mentalThreshold) statsPassedLimit.Add(CharacterStat.CHA);
		if (willpower <= mentalThreshold) statsPassedLimit.Add(CharacterStat.WIL);
		if (money <= debtThreshold) statsPassedLimit.Add(CharacterStat.MONEY);
		return statsPassedLimit;
	}
}

[Serializable]
public struct StatChange {
	public CharacterStat stat;
	public int value;
	public bool additive;

	public StatChange(CharacterStat stat, int value, bool additive) {
		this.stat = stat;
		this.value = value;
		this.additive = additive;
	}

	public StatChange(CharacterStat stat, int value) {
		this.stat = stat;
		this.value = value;

		additive = true;
	}
}

public static class DeathTraits {
	public static string From(CharacterStat stat) {
		switch (stat) {
			case CharacterStat.STR:
				return "Strength Limit Death";
			case CharacterStat.TUF:
				return "Toughness Limit Death";
			case CharacterStat.DEX:
				return "Dexterity Limit Death";
			case CharacterStat.PER:
				return "Perception Limit Death";
			case CharacterStat.MAG:
				return "Magic Limit Death";
			case CharacterStat.KNO:
				return "Knowledge Limit Death";
			case CharacterStat.CHA:
				return "Charisma Limit Death";
			case CharacterStat.WIL:
				return "Willpower Limit Death";
			case CharacterStat.LET:
				return "Lethality Limit Death";
			case CharacterStat.MONEY:
				return "Debt Limit Death";
			default:
				return "";
		}
	}
}