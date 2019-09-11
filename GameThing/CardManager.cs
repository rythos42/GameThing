using System.Collections.Generic;
using GameThing.Entities;
using GameThing.Entities.Cards;
using GameThing.Entities.Cards.Conditions;

namespace GameThing
{
	public class CardManager
	{
		public List<Card> CreateDefaultDeck(Character character)
		{
			var deck = new List<Card>
			{
				new Card {
					Title = "Clumsy Stab",
					Description = "Deal 100% Str damage at range 1.",
					Range = 1,
					OwnerCharacter = character,
					CardType = Type.Damage,
					AbilityScore = AbilityScore.Strength,
					EffectPercent = 1,
					Categories = new List<Category> { Category.Strength, Category.Damage, Category.Melee }
				},
				new Card
				{
					Title = "Bad Bandage",
					Description = "Heal 100% Int to self.",
					Range = 0,
					OwnerCharacter = character,
					CardType = Type.Heal,
					AbilityScore = AbilityScore.Intelligence,
					EffectPercent = 1,
					Categories = new List<Category> { Category.Intelligence, Category.Healing, Category.Self },
				},
				new Card {
					Title = "Awkward Distraction",
					Description = "Apply a condition at range 1: If the next attack is from another character, it gains +200% Str.",
					Range = 1,
					OwnerCharacter = character,
					CardType = Type.Condition,
					Condition = new DistractCondition(AbilityScore.Strength, 3) { SourceCharacter = character },
					Categories = new List<Category> { Category.Support, Category.Melee, Category.Strength }
				},
				new Card
				{
					Title = "Lame Taunt",
					Description = "Apply a condition at range 5: 70% chance next card must target this character.",
					Range = 5,
					OwnerCharacter = character,
					CardType = Type.Condition,
					Condition = new TauntCondition(0.7m) { SourceCharacter = character },
					Categories = new List<Category> { Category.Support, Category.Reckless, Category.Ranged }
				},
				new Card
				{
					Title = "Slow Jog",
					Description = "Apply a condition to yourself: Your next move gains +1 distance.",
					Range = 0,
					OwnerCharacter = character,
					CardType = Type.Condition,
					Condition = new RunCondition(1) { SourceCharacter = character },
					Categories = new List<Category> { Category.Movement, Category.Support, Category.Self }
				},
				new Card
				{
					Title = "Weak Cheer",
					Description = "Apply a condition at range 3: Give another character +100% Str for 3 turns.",
					Range = 3,
					OwnerCharacter = character,
					CardType = Type.Condition,
					Condition = new BuffCondition(AbilityScore.Strength, 2, 3, BuffType.Percent) { SourceCharacter = character },
					Categories = new List<Category> { Category.Support, Category.Ranged, Category.Strength, Category.Duration }
				}
			};

			switch (character.CharacterClass)
			{
				case CharacterClass.Squire:
					deck.Add(new Card
					{
						Title = "Slightly Trained Stab",
						Description = "Deal 120% Str damage at range 1.",
						Range = 1,
						OwnerCharacter = character,
						CardType = Type.Damage,
						AbilityScore = AbilityScore.Strength,
						EffectPercent = 1.2m,
						Categories = new List<Category> { Category.Strength, Category.Damage, Category.Melee }
					});
					deck.Add(new Card
					{
						Title = "Prepared Stance",
						Description = "Apply a condition to yourself: Gain +3 Health for 5 turns.",
						Range = 0,
						OwnerCharacter = character,
						CardType = Type.Condition,
						Condition = new BuffCondition(AbilityScore.Health, 3, 5, BuffType.Linear) { SourceCharacter = character },
						Categories = new List<Category> { Category.Support, Category.Defense, Category.Healing }
					});
					break;

				case CharacterClass.Pickpocket:
					deck.Add(new Card
					{
						Title = "Decent Slice",
						Description = "Deal 120% Agi damage at range 1.",
						Range = 1,
						OwnerCharacter = character,
						CardType = Type.Damage,
						AbilityScore = AbilityScore.Agility,
						EffectPercent = 1.2m,
						Categories = new List<Category> { Category.Agility, Category.Damage, Category.Melee }
					});
					deck.Add(new Card
					{
						Title = "Slipped Backstab",
						Description = "Deal 150% Agi damage at range 1.",
						Range = 1,
						OwnerCharacter = character,
						CardType = Type.Damage,
						AbilityScore = AbilityScore.Agility,
						EffectPercent = 1.5m,
						Categories = new List<Category> { Category.Agility, Category.Damage, Category.Melee }
					});
					break;

				case CharacterClass.Apprentice:
					deck.Add(new Card
					{
						Title = "Fairie Fire",
						Description = "Does nothing. You need to train magic to use it!",
						Range = 0,
						OwnerCharacter = character,
						CardType = Type.Damage,
						AbilityScore = AbilityScore.Intelligence,
						EffectPercent = 0,
						Categories = new List<Category> { Category.Intelligence, Category.Magic }
					});
					deck.Add(new Card
					{
						Title = "Arcane Missile",
						Description = "Deal 100% Int damage at range 5.",
						Range = 5,
						OwnerCharacter = character,
						CardType = Type.Damage,
						AbilityScore = AbilityScore.Intelligence,
						EffectPercent = 1,
						Categories = new List<Category> { Category.Intelligence, Category.Damage, Category.Ranged, Category.Magic }
					});
					break;
			}

			return deck;
		}
	}
}
