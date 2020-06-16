using System;
using System.Collections.Generic;
using GameThing.Database;
using GameThing.Entities.Cards;
using NUnit.Framework;

namespace GameThing.Tests.Database
{
	[TestFixture]
	public class CharacterClassMapperTests
	{
		[Test]
		public void LoadClasses_LoadsDataStructure()
		{
			string jsonString = @"
                [
                    {
                        ""id"": ""apprentice"",
                        ""name"": ""Apprentice"",
                        ""cards"": [ ""1"", ""2"", ""3"", ""4"", ""5"", ""6"", ""11"", ""12"" ]
                    }
                ]
            ";

			var originalCardMapper = CardMapper.Instance;
			CardMapper.Instance = new TestCardMapper();
			var classMapper = CharacterClassMapper.Instance;
			classMapper.LoadClasses(jsonString);

			var characterClass = classMapper.Get("apprentice");
			Assert.That(characterClass.Name, Is.EqualTo("Apprentice"));
			Assert.That(characterClass.Cards, Is.EqualTo(new List<string> { "1", "2", "3", "4", "5", "6", "11", "12" }));

			CardMapper.Instance = originalCardMapper;
		}

		[Test]
		public void LoadClasses_ThrowsIfDuplicateIds()
		{
			string jsonString = @"
                [
                    {
                        ""id"": ""apprentice"",
                        ""name"": ""Apprentice"",
                        ""startingCards"": [ ""1"", ""2"", ""3"", ""4"", ""5"", ""6"", ""11"", ""12"" ]
                    },
                    {
                        ""id"": ""apprentice"",
                        ""name"": ""Apprentice"",
                        ""startingCards"": [ ""1"", ""2"", ""3"", ""4"", ""5"", ""6"", ""11"", ""12"" ]
                    }
                ]
            ";

			var classMapper = CharacterClassMapper.Instance;
			Assert.Throws<Exception>(() => classMapper.LoadClasses(jsonString), "One of the class IDs in classes.json is not unique.");
		}

		private class TestCardMapper : ICardMapper
		{
			public Card Get(string id)
			{
				return new Card { Id = id };
			}

			public void Load()
			{
				throw new NotImplementedException();
			}
		}

		[Test]
		public void LoadClasses_ThrowsIfCardIdNotInCardDatabase()
		{
			string jsonString = @"
                [
                    {
                        ""id"": ""apprentice"",
                        ""name"": ""Apprentice"",
                        ""startingCards"": [ ""1"", ""2"", ""3"", ""4"", ""5"", ""6"", ""11"", ""12"" ]
                    }
                ]
            ";

			var classMapper = CharacterClassMapper.Instance;
			Assert.Throws<Exception>(() => classMapper.LoadClasses(jsonString), "One of the starting card IDs in classes.json does not exist in cards.json.");
		}
	}
}
