﻿using System.Collections.Generic;
using GameThing.Entities;
using GameThing.Entities.Cards;
using GameThing.Entities.Cards.Conditions;

namespace GameThing
{
	public class CardManager
	{
		public List<Card> CreateDefaultDeck(Character character)
		{
			var basicDistract = new DistractCondition(1, AbilityScore.Strength, 3);
			var basicTaunt = new TauntCondition(2, 0.7m);
			var basicRun = new RunCondition(3, 1);
			var basicStrengthBuff = new BuffCondition(4, AbilityScore.Strength, 2, 3, BuffType.Percent);
			var basicHealthBuff = new BuffCondition(5, AbilityScore.Health, 3, 5, BuffType.Linear);

			var deck = new List<Card>
			{
				new DamageCard(1, "Clumsy Stab", "Deal 100% Str damage at range 1.", 1, character, AbilityScore.Strength, 1),
				new HealCard(2, "Bad Bandage", "Heal 100% Int to self.", 0, character, AbilityScore.Intelligence, 1),
				new ConditionCard(3, "Awkward Distraction", "Apply a condition at range 1: If the next attack is from another character, it gains +200% Str.", 1, character,basicDistract),
				new ConditionCard(4, "Lame Taunt", "Apply a condition at range 5: 70% chance next card must target this character.", 5, character, basicTaunt),
				new ConditionCard(5, "Slow Jog", "Apply a condition to yourself: Your next move gains +1 distance.", 0, character, basicRun),
				new ConditionCard(6, "Weak Cheer", "Apply a condition at range 3: Give another character +100% Str for 3 turns.", 3, character,basicStrengthBuff)
			};

			switch (character.CharacterClass)
			{
				case CharacterClass.Squire:
					deck.Add(new DamageCard(7, "Slightly Trained Stab", "Deal 120% Str damage at range 1.", 1, character, AbilityScore.Strength, 1.2m));
					deck.Add(new ConditionCard(8, "Prepared Stance", "Apply a condition to yourself: Gain +3 Health for 5 turns.", 0, character, basicHealthBuff));
					break;
				case CharacterClass.Pickpocket:
					deck.Add(new DamageCard(9, "Decent Slice", "Deal 120% Agi damage at range 1.", 1, character, AbilityScore.Agility, 1.2m));
					deck.Add(new DamageCard(10, "Slipped Backstab", "Deal 150% Agi damage at range 1.", 1, character, AbilityScore.Agility, 1.5m));
					break;
				case CharacterClass.Apprentice:
					deck.Add(new DamageCard(11, "Fairie Fire", "Does nothing. You need to train magic to use it!", 0, character, AbilityScore.Intelligence, 0));
					deck.Add(new DamageCard(12, "Arcane Missile", "Deal 100% Int damage at range 5.", 5, character, AbilityScore.Intelligence, 1));
					break;
			}
			return deck;
		}
	}
}
