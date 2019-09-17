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

		public static TeamData CreateDefaultTeam()
		{
			var teamData = new TeamData();
			teamData.Characters.Add(CreateCharacter(CharacterColour.Blue));
			teamData.Characters.Add(CreateCharacter(CharacterColour.Green));
			teamData.Characters.Add(CreateCharacter(CharacterColour.None));
			teamData.Characters.Add(CreateCharacter(CharacterColour.Red));
			teamData.Characters.Add(CreateCharacter(CharacterColour.White));
			return teamData;
		}

		private static Character CreateCharacter(CharacterColour colour)
		{
			var classEnumValues = Enum.GetValues(typeof(CharacterClass));
			var thisCharacterClass = new Random().Next(0, classEnumValues.Length);

			var character = new Character(Guid.NewGuid(), colour, (CharacterClass) thisCharacterClass);
			character.InitializeDefaultDeck();
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
