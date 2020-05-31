using System.Collections.Generic;
using System.Linq;
using GameThing.Data;
using GameThing.Entities;
using GameThing.Entities.Cards;
using GameThing.Entities.Cards.Conditions;

namespace GameThing.Manager
{
	public class CardManager
	{
		private readonly List<Card> database;

		private readonly Dictionary<CharacterClass, int[]> startingDecks = new Dictionary<CharacterClass, int[]>
		{
			{ CharacterClass.Squire, new int[] { 1, 2, 3, 4, 5, 6, 7, 8 } },
			{ CharacterClass.Pickpocket, new int[] { 1, 2, 3, 4, 5, 6, 9, 10 } },
			{ CharacterClass.Apprentice, new int[] { 1, 2, 3, 4, 5, 6, 11, 12 } },
		};

		public CardManager()
		{
			var basicDistract = new Condition(1, "If the next attack is from another character, it gains +200% Str.", ConditionType.Distract) { AbilityScore = AbilityScore.Strength, BuffAmount = 3 };
			var basicTaunt = new Condition(2, "70% chance next card must target this character.", ConditionType.Taunt) { SuccessPercent = 0.7m };
			var basicRun = new Condition(3, "Your next move gains +1 distance.", ConditionType.Run) { BuffAmount = 1 };
			var basicStrengthBuff = new Condition(4, "Give another character +100% Str for 3 turns.", ConditionType.Buff)
			{
				AbilityScore = AbilityScore.Strength,
				BuffAmount = 2,
				TurnCount = 3,
				BuffType = BuffType.Percent
			};
			var basicHealthBuff = new Condition(5, "Gain +3 Health for 5 turns.", ConditionType.Buff)
			{
				AbilityScore = AbilityScore.Health,
				BuffAmount = 3,
				TurnCount = 5,
				BuffType = BuffType.Linear
			};

			database = new List<Card> {
				new Card (1)
				{
					Title = "Clumsy Stab",
					Description = "Deal 100% Str (<character.CurrentStrength>) damage at range 1.",
					Range = 1,
					CardType = Type.Damage,
					AbilityScore = AbilityScore.Strength,
					EffectPercent = 1,
					Categories = new List<Category> { Category.Strength, Category.Damage, Category.Melee }
				},
				new Card(2)
				{
					Title = "Bad Bandage",
					Description = "Heal 100% Int (<character.CurrentIntelligence>) to self.",
					Range = 0,
					CardType = Type.Heal,
					AbilityScore = AbilityScore.Intelligence,
					EffectPercent = 1,
					Categories = new List<Category> { Category.Intelligence, Category.Healing, Category.Self },
				},
				new Card (3)
				{
					Title = "Awkward Distraction",
					Description = "Apply a condition at range 1",
					Range = 1,
					CardType = Type.Condition,
					Condition = basicDistract,
					Categories = new List<Category> { Category.Support, Category.Melee, Category.Strength }
				},
				new Card(4)
				{
					Title = "Lame Taunt",
					Description = "Apply a condition at range 5",
					Range = 5,
					CardType = Type.Condition,
					Condition = basicTaunt,
					Categories = new List<Category> { Category.Support, Category.Reckless, Category.Ranged }
				},
				new Card(5)
				{
					Title = "Slow Jog",
					Description = "Apply a condition to yourself",
					Range = 0,
					CardType = Type.Condition,
					Condition = basicRun,
					Categories = new List<Category> { Category.Movement, Category.Support, Category.Self }
				},
				new Card(6)
				{
					Title = "Weak Cheer",
					Description = "Apply a condition at range 3",
					Range = 3,
					CardType = Type.Condition,
					Condition = basicStrengthBuff,
					Categories = new List<Category> { Category.Support, Category.Ranged, Category.Strength, Category.Duration }
				},
				new Card(7)
				{
					Title = "Slightly Trained Stab",
					Description = "Deal 120% Str (<character.CurrentStrength>) damage at range 1.",
					Range = 1,
					CardType = Type.Damage,
					AbilityScore = AbilityScore.Strength,
					EffectPercent = 1.2m,
					Categories = new List<Category> { Category.Strength, Category.Damage, Category.Melee }
				},
				new Card(8)
				{
					Title = "Prepared Stance",
					Description = "Apply a condition to yourself",
					Range = 0,
					CardType = Type.Condition,
					Condition = basicHealthBuff,
					Categories = new List<Category> { Category.Support, Category.Defense, Category.Healing }
				},
				new Card(9)
				{
					Title = "Decent Slice",
					Description = "Deal 120% Agi (<character.CurrentAgility>) damage at range 1.",
					Range = 1,
					CardType = Type.Damage,
					AbilityScore = AbilityScore.Agility,
					EffectPercent = 1.2m,
					Categories = new List<Category> { Category.Agility, Category.Damage, Category.Melee }
				},
				new Card(10)
				{
					Title = "Slipped Backstab",
					Description = "Deal 150% Agi (<character.CurrentAgility>) damage at range 1.",
					Range = 1,
					CardType = Type.Damage,
					AbilityScore = AbilityScore.Agility,
					EffectPercent = 1.5m,
					Categories = new List<Category> { Category.Agility, Category.Damage, Category.Melee }
				},
				new Card(11)
				{
					Title = "Fairie Fire",
					Description = "Does nothing. You need to train magic to use it!",
					Range = 0,
					CardType = Type.Damage,
					AbilityScore = AbilityScore.Intelligence,
					EffectPercent = 0,
					Categories = new List<Category> { Category.Intelligence, Category.Magic }
				},
				new Card(12)
				{
					Title = "Arcane Missile",
					Description = "Deal 100% Int (<character.CurrentIntelligence>) damage at range 5.",
					Range = 5,
					CardType = Type.Damage,
					AbilityScore = AbilityScore.Intelligence,
					EffectPercent = 1,
					Categories = new List<Category> { Category.Intelligence, Category.Damage, Category.Ranged, Category.Magic }
				}
			};
		}

		public Card GetCard(int id)
		{
			return database.SingleOrDefault(card => card.Id == id);
		}

		public List<Card> CreateDefaultDeck(Character character)
		{
			return startingDecks[character.CharacterClass].Select(cardId => GetCard(cardId).CreateForCharacter(character)).ToList();
		}
	}
}
