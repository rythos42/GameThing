using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GameThing.Database;
using GameThing.Entities;

namespace GameThing.Contract
{
	[DataContract]
	public class TeamData
	{
		[DataMember]
		public List<Character> Characters { get; set; } = new List<Character>();

		[DataMember]
		public string OwnerPlayerId { get; set; }

		public static TeamData CreateDefaultTeam()
		{
			NameMapper.Instance.Load();
			var squireClass = CharacterClassMapper.Instance.Get("squire");
			var apprenticeClass = CharacterClassMapper.Instance.Get("apprentice");
			var pickpocketClass = CharacterClassMapper.Instance.Get("pickpocket");

			var teamData = new TeamData { OwnerPlayerId = ApplicationData.PlayerId };
			teamData.Characters.Add(CreateCharacter(CharacterColour.Blue, squireClass));
			teamData.Characters.Add(CreateCharacter(CharacterColour.Green, squireClass));
			teamData.Characters.Add(CreateCharacter(CharacterColour.None, apprenticeClass));
			teamData.Characters.Add(CreateCharacter(CharacterColour.Red, apprenticeClass));
			teamData.Characters.Add(CreateCharacter(CharacterColour.White, pickpocketClass));
			return teamData;
		}

		private static Character CreateCharacter(CharacterColour colour, CharacterClass cClass)
		{
			var character = new Character(Guid.NewGuid(), colour, cClass)
			{
				OwnerPlayerId = ApplicationData.PlayerId,
				Name = NameMapper.Instance.GetRandom()
			};
			character.InitializeDefaultDeck();
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
