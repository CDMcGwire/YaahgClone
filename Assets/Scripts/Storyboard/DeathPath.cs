using UnityEngine;

public class DeathPath : Path {
	[SerializeField]
	private Panel strengthDeath;
	[SerializeField]
	private Panel toughnessDeath;
	[SerializeField]
	private Panel dexterityDeath;
	[SerializeField]
	private Panel perceptionDeath;
	[SerializeField]
	private Panel magicDeath;
	[SerializeField]
	private Panel knowledgeDeath;
	[SerializeField]
	private Panel charismaDeath;
	[SerializeField]
	private Panel willpowerDeath;
	[SerializeField]
	private Panel debtDeath;
	[SerializeField]
	private Panel physicalDeath;
	[SerializeField]
	private Panel mentalDeath;
	[SerializeField]
	private Panel physmentDeath;
	[SerializeField]
	private Panel physdebtDeath;
	[SerializeField]
	private Panel mentdebtDeath;
	[SerializeField]
	private Panel completeDeath;

	[SerializeField]
	private Panel partyWipe;

	public override Panel GetNextPanel(GameData gameData) {
		if (gameData.Characters.Count < 1) return null;
		if (gameData.Characters.Count > 1) return partyWipe;

		var character = gameData.Characters[0]; // Only checks first character
		var limits = character.Data.GetLimitsReached();

		if (limits.Count == 1) {
			switch (limits[0]) {
				case CharacterStat.STR:
					return strengthDeath;
				case CharacterStat.TUF:
					return toughnessDeath;
				case CharacterStat.DEX:
					return dexterityDeath;
				case CharacterStat.PER:
					return perceptionDeath;
				case CharacterStat.MAG:
					return magicDeath;
				case CharacterStat.KNO:
					return knowledgeDeath;
				case CharacterStat.CHA:
					return charismaDeath;
				case CharacterStat.WIL:
					return willpowerDeath;
				case CharacterStat.MONEY:
					return debtDeath;
			}
		}
		else {
			var physicalLimit = false;
			var mentalLimit = false;
			var debtLimit = false;

			foreach (var limit in limits) {
				switch (limit) {
					case CharacterStat.STR:
					case CharacterStat.TUF:
					case CharacterStat.DEX:
					case CharacterStat.PER:
						physicalLimit = true;
						break;
					case CharacterStat.MAG:
					case CharacterStat.KNO:
					case CharacterStat.CHA:
					case CharacterStat.WIL:
						mentalLimit = true;
						break;
					case CharacterStat.MONEY:
						debtLimit = true;
						break;
				}
			}

			if (physicalLimit && mentalLimit && debtLimit) return completeDeath;
			else if (physicalLimit && mentalLimit) return physmentDeath;
			else if (physicalLimit && debtLimit) return physdebtDeath;
			else if (mentalLimit && debtLimit) return mentdebtDeath;
			else if (physicalLimit) return physicalDeath;
			else if (mentalLimit) return mentalDeath;
			else if (debtLimit) return debtDeath;
		}
		return null;
	}
}