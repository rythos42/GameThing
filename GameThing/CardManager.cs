﻿using System.Collections.Generic;
using System.Linq;
using GameThing.Entities;
using GameThing.Entities.Cards;
using GameThing.Entities.Cards.Conditions;

namespace GameThing
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
			var basicDistract = new DistractCondition(1, AbilityScore.Strength, 3);
			var basicTaunt = new TauntCondition(2, 0.7m);
			var basicRun = new RunCondition(3, 1);
			var basicStrengthBuff = new BuffCondition(4, AbilityScore.Strength, 2, 3, BuffType.Percent);
			var basicHealthBuff = new BuffCondition(5, AbilityScore.Health, 3, 5, BuffType.Linear);

			database = new List<Card> {
				new Card (1) {
					Title = "Clumsy Stab",
					Description = "Deal 100% Str damage at range 1.",
					Range = 1,
					CardType = Type.Damage,
					AbilityScore = AbilityScore.Strength,
					EffectPercent = 1,
					Categories = new List<Category> { Category.Strength, Category.Damage, Category.Melee }
				},
				new Card(2)
				{
					Title = "Bad Bandage",
					Description = "Heal 100% Int to self.",
					Range = 0,
					CardType = Type.Heal,
					AbilityScore = AbilityScore.Intelligence,
					EffectPercent = 1,
					Categories = new List<Category> { Category.Intelligence, Category.Healing, Category.Self },
				},
				new Card (3){
					Title = "Awkward Distraction",
					Description = "Apply a condition at range 1: If the next attack is from another character, it gains +200% Str.",
					Range = 1,
					CardType = Type.Condition,
					Condition = basicDistract,
					Categories = new List<Category> { Category.Support, Category.Melee, Category.Strength }
				},
				new Card(4)
				{
					Title = "Lame Taunt",
					Description = "Apply a condition at range 5: 70% chance next card must target this character.",
					Range = 5,
					CardType = Type.Condition,
					Condition = basicTaunt,
					Categories = new List<Category> { Category.Support, Category.Reckless, Category.Ranged }
				},
				new Card(5)
				{
					Title = "Slow Jog",
					Description = "Apply a condition to yourself: Your next move gains +1 distance.",
					Range = 0,
					CardType = Type.Condition,
					Condition = basicRun,
					Categories = new List<Category> { Category.Movement, Category.Support, Category.Self }
				},
				new Card(6)
				{
					Title = "Weak Cheer",
					Description = "Apply a condition at range 3: Give another character +100% Str for 3 turns.",
					Range = 3,
					CardType = Type.Condition,
					Condition = basicStrengthBuff,
					Categories = new List<Category> { Category.Support, Category.Ranged, Category.Strength, Category.Duration }
				},
				new Card(7)
				{
					Title = "Slightly Trained Stab",
					Description = "Deal 120% Str damage at range 1.",
					Range = 1,
					CardType = Type.Damage,
					AbilityScore = AbilityScore.Strength,
					EffectPercent = 1.2m,
					Categories = new List<Category> { Category.Strength, Category.Damage, Category.Melee }
				},
				new Card(8)
				{
					Title = "Prepared Stance",
					Description = "Apply a condition to yourself: Gain +3 Health for 5 turns.",
					Range = 0,
					CardType = Type.Condition,
					Condition = basicHealthBuff,
					Categories = new List<Category> { Category.Support, Category.Defense, Category.Healing }
				},
				new Card(9)
				{
					Title = "Decent Slice",
					Description = "Deal 120% Agi damage at range 1.",
					Range = 1,
					CardType = Type.Damage,
					AbilityScore = AbilityScore.Agility,
					EffectPercent = 1.2m,
					Categories = new List<Category> { Category.Agility, Category.Damage, Category.Melee }
				},
				new Card(10)
				{
					Title = "Slipped Backstab",
					Description = "Deal 150% Agi damage at range 1.",
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
					Description = "Deal 100% Int damage at range 5.",
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
