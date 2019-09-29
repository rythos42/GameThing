using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GameThing.Entities;

namespace GameThing.Data
{
	[DataContract]
	public class TeamData
	{
		[DataMember]
		public List<Character> Characters { get; set; } = new List<Character>();

		public static TeamData CreateDefaultTeam(CardManager cardManager)
		{
			var teamData = new TeamData();
			teamData.Characters.Add(CreateCharacter(CharacterColour.Blue, CharacterClass.Squire, cardManager));
			teamData.Characters.Add(CreateCharacter(CharacterColour.Green, CharacterClass.Squire, cardManager));
			teamData.Characters.Add(CreateCharacter(CharacterColour.None, CharacterClass.Apprentice, cardManager));
			teamData.Characters.Add(CreateCharacter(CharacterColour.Red, CharacterClass.Apprentice, cardManager));
			teamData.Characters.Add(CreateCharacter(CharacterColour.White, CharacterClass.Pickpocket, cardManager));
			return teamData;
		}

		private static Character CreateCharacter(CharacterColour colour, CharacterClass cClass, CardManager cardManager)
		{
			var character = new Character(Guid.NewGuid(), colour, cClass);
			character.InitializeDefaultDeck(cardManager);
			return character;
		}

		private Character GetById(Guid id)
		{
			return Characters.SingleOrDefault(character => character.Id == id);
		}

		public void MergeBattleData(BattleData battleData)
		{
			foreach (var battleCharacter in battleData.Characters)
			{
				foreach (var battleCategory in battleCharacter.AdditionalCategoryLevels.Keys)
				{
					var teamCharacter = GetById(battleCharacter.Id);
					var additionalCategoryLevelFromBattle = battleCharacter.AdditionalCategoryLevels[battleCategory];

					if (teamCharacter.CategoryLevels.ContainsKey(battleCategory))
						teamCharacter.CategoryLevels[battleCategory] += additionalCategoryLevelFromBattle;
					else
						teamCharacter.CategoryLevels.Add(battleCategory, additionalCategoryLevelFromBattle);
				}
			}
		}
	}
}
