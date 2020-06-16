using System;
using GameThing.Contract;
using GameThing.Database;
using GameThing.Entities.Cards;
using GameThing.Entities.Cards.Conditions;
using NUnit.Framework;

namespace GameThing.Tests.Database
{
	[TestFixture]
	public class CardMapperTests
	{
		[Test]
		public void LoadCards_LoadsDataStructure()
		{
			var jsonString = @"
                [
                    {
                        ""id"": 8,
                        ""title"": ""Prepared Stance"",
                        ""cardType"": ""Condition"",
                        ""description"": ""Apply a condition to yourself"",
                        ""range"": 1,
                        ""categories"": [
                            ""support"",
                            ""defense"",
                            ""healing""
                        ],
                        ""condition"": {
                            ""stackGroup"": ""5"",
                            ""text"": ""Gain +3 Health for 5 turns."",
                            ""turnCount"": 5,
                            ""endsOn"": ""StartRound"",
                            ""iconName"": ""sprites/icons/buff"",
                            ""successPercent"": 1,
                            ""effects"": [
                                {
                                    ""type"": ""Buff"",
                                    ""abilityScore"": ""Health"",
                                    ""buffAmount"": 3,
                                    ""buffType"": ""Linear"",
                                }
                            ]
                        }
                    }
                ]
            ";
			var originalCategoryMapper = CategoryMapper.Instance;
			CategoryMapper.Instance = new TestCategoryMapper();
			var cardMapper = (CardMapper) CardMapper.Instance;
			cardMapper.LoadCards(jsonString);

			var returnedCard = cardMapper.Get("8");
			Assert.That(returnedCard.Title, Is.EqualTo("Prepared Stance"));
			Assert.That(returnedCard.CardType, Is.EqualTo(CardType.Condition));
			Assert.That(returnedCard.Description, Is.EqualTo("Apply a condition to yourself"));
			Assert.That(returnedCard.Range, Is.EqualTo(1));
			Assert.That(returnedCard.Categories.Count, Is.EqualTo(3));
			Assert.That(returnedCard.Categories[0].Id, Is.EqualTo("support"));
			Assert.That(returnedCard.Categories[0].Name, Is.EqualTo("Support"));
			Assert.That(returnedCard.Categories[1].Id, Is.EqualTo("defense"));
			Assert.That(returnedCard.Categories[1].Name, Is.EqualTo("Defense"));
			Assert.That(returnedCard.Categories[2].Id, Is.EqualTo("healing"));
			Assert.That(returnedCard.Categories[2].Name, Is.EqualTo("Healing"));
			Assert.That(returnedCard.Condition.StackGroup, Is.EqualTo("5"));
			Assert.That(returnedCard.Condition.Text, Is.EqualTo("Gain +3 Health for 5 turns."));
			Assert.That(returnedCard.Condition.TurnCount, Is.EqualTo(5));
			Assert.That(returnedCard.Condition.EndsOn, Is.EqualTo(ConditionEndsOn.StartRound));
			Assert.That(returnedCard.Condition.IconName, Is.EqualTo("sprites/icons/buff"));
			Assert.That(returnedCard.Condition.SuccessPercent, Is.EqualTo(1));
			Assert.That(returnedCard.Condition.Effects.Count, Is.EqualTo(1));
			Assert.That(returnedCard.Condition.Effects[0].Type, Is.EqualTo(EffectType.Buff));
			Assert.That(returnedCard.Condition.Effects[0].AbilityScore, Is.EqualTo(AbilityScore.Health));
			Assert.That(returnedCard.Condition.Effects[0].BuffAmount, Is.EqualTo(3));
			Assert.That(returnedCard.Condition.Effects[0].BuffType, Is.EqualTo(BuffType.Linear));

			CategoryMapper.Instance = originalCategoryMapper;
		}

		[Test]
		public void LoadCards_ThrowsIfDuplicateIds()
		{
			var jsonString = @"
                [
                    { ""id"": 8 },
                    { ""id"": 8 }
                ]
            ";
			var originalCategoryMapper = CategoryMapper.Instance;
			CategoryMapper.Instance = new TestCategoryMapper();

			var cardMapper = (CardMapper) CardMapper.Instance;
			Assert.Throws<Exception>(() => cardMapper.LoadCards(jsonString), "One of the card IDs in cards.json is not unique.");

			CategoryMapper.Instance = originalCategoryMapper;
		}

		private class TestCategoryMapper : ICategoryMapper
		{
			public Category GetCategory(string id)
			{
				switch (id)
				{
					case "support": return new Category { Id = "support", Name = "Support" };
					case "defense": return new Category { Id = "defense", Name = "Defense" };
					case "healing": return new Category { Id = "healing", Name = "Healing" };
				}
				throw new Exception("Not testable");
			}

			public Category Get(string id)
			{
				return GetCategory(id);
			}

			public void Load()
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
