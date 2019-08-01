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
				new DamageCard("Clumsy Stab", "Deal 100% Str damage at range 1.", 1, character, AbilityScore.Strength, 1),
				new HealCard("Bad Bandage", "Heal 100% Int to self.", 0, character, AbilityScore.Intelligence, 1),
				new DamageCard("Fairie Fire", "Does nothing. You need to train magic to use it!", 0, character, AbilityScore.Intelligence, 0),
				new ConditionCard("Awkward Distraction", "Apply a condition at range 1: If the next attack is from another character, it gains +200% Str.", 1, character, new DistractCondition(AbilityScore.Strength, 3)),
				new ConditionCard("Lame Taunt", "Apply a condition at range 1: 70% chance next card must target this character.", 1, character, new TauntCondition(0.7m)),
				new ConditionCard("Slow Jog", "Apply a condition to yourself: Your next move gains +1 distance.", 0, character, new RunCondition(1)),
				new ConditionCard("Weak Cheer", "Apply a condition at range 1: Give another character +100% Str for 3 turns.", 1, character, new BuffCondition(AbilityScore.Strength, 2, 3))
			};

			switch (character.CharacterClass)
			{
				case CharacterClass.Squire: deck.Add(new DamageCard("Slightly Trained Stab", "Deal 120% Str damage at range 1.", 1, character, AbilityScore.Strength, 1.2m)); break;
				case CharacterClass.Pickpocket: deck.Add(new DamageCard("Slipped Backstab", "Deal 150% Agi damage at range 1.", 1, character, AbilityScore.Agility, 1.5m)); break;
				case CharacterClass.Apprentice: deck.Add(new DamageCard("Arcane Missile", "Deal 100% Int damage at range 5.", 5, character, AbilityScore.Intelligence, 1)); break;
			}
			return deck;
		}
	}
}
