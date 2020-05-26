using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GameThing.Entities;
using GameThing.Manager;

namespace GameThing.Data
{
	[DataContract]
	public class TeamData
	{
		[DataMember]
		public List<Character> Characters { get; set; } = new List<Character>();

		[DataMember]
		public string OwnerPlayerId { get; set; }

		public static TeamData CreateDefaultTeam(CardManager cardManager)
		{
			var teamData = new TeamData { OwnerPlayerId = ApplicationData.PlayerId };
			teamData.Characters.Add(CreateCharacter(CharacterColour.Blue, CharacterClass.Squire, cardManager));
			teamData.Characters.Add(CreateCharacter(CharacterColour.Green, CharacterClass.Squire, cardManager));
			teamData.Characters.Add(CreateCharacter(CharacterColour.None, CharacterClass.Apprentice, cardManager));
			teamData.Characters.Add(CreateCharacter(CharacterColour.Red, CharacterClass.Apprentice, cardManager));
			teamData.Characters.Add(CreateCharacter(CharacterColour.White, CharacterClass.Pickpocket, cardManager));
			return teamData;
		}

		private static Character CreateCharacter(CharacterColour colour, CharacterClass cClass, CardManager cardManager)
		{
			var character = new Character(Guid.NewGuid(), colour, cClass) { OwnerPlayerId = ApplicationData.PlayerId };
			character.InitializeDefaultDeck(cardManager);
			return character;
		}

		private Character GetCharacter(Guid id)
		{
			return Characters.SingleOrDefault(character => character.Id == id);
		}

		public void MergeBattleData(BattleData battleData)
		{
			foreach (var battleCharacter in battleData.Characters)
			{
				if (battleCharacter.OwnerPlayerId != OwnerPlayerId)
					continue;

				foreach (var category in battleCharacter.AdditionalCategoryLevels.Keys)
				{
					var teamCharacter = GetCharacter(battleCharacter.Id);
					var additionalCategoryLevelFromBattle = battleCharacter.AdditionalCategoryLevels[category];

					if (teamCharacter.CategoryLevels.ContainsKey(category))
						teamCharacter.CategoryLevels[category] += additionalCategoryLevelFromBattle;
					else
						teamCharacter.CategoryLevels.Add(category, additionalCategoryLevelFromBattle);
				}
			}
		}
	}
}
