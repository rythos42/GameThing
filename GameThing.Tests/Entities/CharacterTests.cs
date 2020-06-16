using System;
using System.Collections.Generic;
using GameThing.Contract;
using GameThing.Entities;
using GameThing.Entities.Cards;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace GameThing.Tests.Entities
{
	[TestFixture]
	public class CharacterTests
	{
		[Test]
		public void Serializes_CharacterClassAsCharacterClassId_NotEntireObject()
		{
			var character = CreateTestCharacter();
			var characterJson = JsonConvert.SerializeObject(character);
			var characterJObj = JObject.Parse(characterJson);

			var serializedCharacterClass = characterJObj.GetValue("CharacterClass").ToString();
			Assert.That(serializedCharacterClass, Is.EqualTo("apprentice"));
		}

		[Test]
		public void Serializes_DeckAsListOfCardIds_NotEntireObject()
		{
			var character = CreateTestCharacter();
			character.Deck.Add(new Card { Id = "1" });
			var characterJson = JsonConvert.SerializeObject(character);
			var characterJObj = JObject.Parse(characterJson);

			var serializedDeck = characterJObj.GetValue("Deck").ToObject<List<string>>();
			Assert.That(serializedDeck.Count, Is.EqualTo(1));
			Assert.That(serializedDeck[0], Is.EqualTo("1"));
		}

		[Test]
		public void AttemptToApplyDamage_AccountsForDefense()
		{
			var character = CreateTestCharacter();
			character.SetAbilityScoreMultiplier(AbilityScore.Evade, 0);  // Ignore Evade

			character.SetCurrentHealth(20);
			character.SetAbilityScoreMultiplier(AbilityScore.Defense, 2);
			var playStatus = character.AttemptToApplyDamage(10);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(11));

			character.SetCurrentHealth(20);
			character.SetAbilityScoreMultiplier(AbilityScore.Defense, 3);
			character.AttemptToApplyDamage(10);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(12));

			character.SetCurrentHealth(20);
			character.SetAbilityScoreMultiplier(AbilityScore.Defense, 4);
			character.AttemptToApplyDamage(10);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(13));

			character.SetCurrentHealth(20);
			character.SetAbilityScoreMultiplier(AbilityScore.Defense, 5);
			character.AttemptToApplyDamage(10);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(14));

			character.SetCurrentHealth(20);
			character.SetAbilityScoreMultiplier(AbilityScore.Defense, 6);
			character.AttemptToApplyDamage(10);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(15));
		}

		[Test]
		public void AttemptToApplyDamage_ReturnsInformationAboutPlay_ForDefense()
		{
			var character = CreateTestCharacter();
			character.SetAbilityScoreMultiplier(AbilityScore.Evade, 0);  // Ignore Evade

			character.SetCurrentHealth(20);
			character.SetAbilityScoreMultiplier(AbilityScore.Defense, 2);
			var playStatus = character.AttemptToApplyDamage(10);
			Assert.That(playStatus.CardType, Is.EqualTo(CardType.Damage));
			Assert.That(playStatus.PlayCancelled, Is.EqualTo(false));
			Assert.That(playStatus.Status, Is.EqualTo(PlayStatusDetails.Success));
			Assert.That(playStatus.ActualDamageOrHealingDone, Is.EqualTo(9));
		}

		[Test]
		public void AttemptToApplyDamage_AccountsForEvade()
		{
			var character = CreateTestCharacter();
			character.SetAbilityScoreMultiplier(AbilityScore.Defense, 1); // Ensure no defense

			var testRandom = new TestRandomWrapper();
			Character.Random = testRandom;

			// Evade 1 is 2.5% chance of miss
			character.SetAbilityScoreMultiplier(AbilityScore.Evade, 1);

			character.SetCurrentHealth(20);
			testRandom.Double = 0.025d;   // 0.025 roll > 0.025 evade is false, so no damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(20));

			character.SetCurrentHealth(20);
			testRandom.Double = 0.0251d;   // 0.0251 roll > 0.025 evade is true, so damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(0));

			// Evade 2 is 10% chance of miss
			character.SetAbilityScoreMultiplier(AbilityScore.Evade, 2);

			character.SetCurrentHealth(20);
			testRandom.Double = 0.1d;   // 0.1 roll > 0.1 evade is false, so no damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(20));

			character.SetCurrentHealth(20);
			testRandom.Double = 0.11d;   // 0.11 roll > 0.1 evade is true, so damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(0));


			// Evade 3 is 22.5% chance of miss
			character.SetAbilityScoreMultiplier(AbilityScore.Evade, 3);

			character.SetCurrentHealth(20);
			testRandom.Double = 0.225d;   // 0.225 roll > 0.225 evade is false, so no damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(20));

			character.SetCurrentHealth(20);
			testRandom.Double = 0.2251d;   // 0.2251 roll > 0.225 evade is true, so damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(0));


			// Evade 4 is 40% chance of msis
			character.SetAbilityScoreMultiplier(AbilityScore.Evade, 4);

			character.SetCurrentHealth(20);
			testRandom.Double = 0.4d;   // 0.4 roll > 0.4 evade is false, so no damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(20));

			character.SetCurrentHealth(20);
			testRandom.Double = 41;   // 0.41 roll > 0.4 evade is true, so damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(0));
		}

		[Test]
		public void AttemptToApplyDamage_ReturnsInformationAboutPlay_ForEvade()
		{
			var character = CreateTestCharacter();
			character.SetAbilityScoreMultiplier(AbilityScore.Defense, 1); // Ensure no defense

			var testRandom = new TestRandomWrapper();
			Character.Random = testRandom;

			// Evade 1 is 2.5% chance of miss
			character.SetAbilityScoreMultiplier(AbilityScore.Evade, 1);

			character.SetCurrentHealth(20);
			testRandom.Double = 0.025d;   // 0.025 roll > 0.025 evade is false, so no damage
			var playStatus = character.AttemptToApplyDamage(20);
			Assert.That(playStatus.CardType, Is.EqualTo(CardType.Damage));
			Assert.That(playStatus.PlayCancelled, Is.EqualTo(false));
			Assert.That(playStatus.Status, Is.EqualTo(PlayStatusDetails.FailedEvaded));
			Assert.That(playStatus.ActualDamageOrHealingDone, Is.EqualTo(0));

			character.SetCurrentHealth(20);
			testRandom.Double = 0.0251d;   // 0.0251 roll > 0.025 evade is true, so damage
			playStatus = character.AttemptToApplyDamage(20);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(0));
			Assert.That(playStatus.CardType, Is.EqualTo(CardType.Damage));
			Assert.That(playStatus.PlayCancelled, Is.EqualTo(false));
			Assert.That(playStatus.Status, Is.EqualTo(PlayStatusDetails.Success));
			Assert.That(playStatus.ActualDamageOrHealingDone, Is.EqualTo(20));
		}

		[Test]
		public void SetsAndGetsAbilityScores()
		{
			var character = CreateTestCharacter();

			Assert.That(character.GetBaseAbilityScore(AbilityScore.Strength), Is.EqualTo(1));
			Assert.That(character.GetCurrentAbilityScore(AbilityScore.Strength), Is.EqualTo(1));

			character.SetAbilityScoreMultiplier(AbilityScore.Strength, 2);
			Assert.That(character.GetAbilityScoreMultiplier(AbilityScore.Strength), Is.EqualTo(2));
			Assert.That(character.GetCurrentAbilityScore(AbilityScore.Strength), Is.EqualTo(2));
		}

		[Test]
		public void AppliesHealing_DoesNotHealAboveMaxHealth()
		{
			var character = CreateTestCharacter();

			character.SetCurrentHealth(6);
			character.ApplyHealing(1);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(7));

			character.SetCurrentHealth(6);
			character.ApplyHealing(1000);
			Assert.That(character.GetBaseAbilityScore(AbilityScore.Health), Is.EqualTo(character.CurrentMaxHealth));
		}

		[Test]
		public void ChangeDamageOrHealing_AccountsForStamina_WithWorstCaseLastPlayedTime()
		{
			var character = CreateTestCharacter();
			character.CharactersInGameCount = 10;
			character.EndTurn(1);

			Assert.That(character.ChangeDamageOrHealingForStamina(100, 2), Is.EqualTo(50m));

			character.SetAbilityScoreMultiplier(AbilityScore.Stamina, 1.1m);
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 2), Is.EqualTo(54.54545454545454545454545454m));

			character.SetAbilityScoreMultiplier(AbilityScore.Stamina, 2m);
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 2), Is.EqualTo(75m));

			character.SetAbilityScoreMultiplier(AbilityScore.Stamina, 3m);
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 2), Is.EqualTo(83.33333333333333333333333334m));

			character.SetAbilityScoreMultiplier(AbilityScore.Stamina, 4);
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 2), Is.EqualTo(87.5m));
		}

		[Test]
		public void ChangeDamageOrHealing_AccountsForLastPlayedTime_WithoutStaminaChange()
		{
			var character = CreateTestCharacter();
			character.CharactersInGameCount = 10;

			// No change if the character hasn't gone yet.
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 9), Is.EqualTo(100m));

			// Set the last turn this character moved.
			character.EndTurn(1);

			// Give gradually more power the further away from your last turn it is.
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 2), Is.EqualTo(50m));
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 3), Is.EqualTo(55m));
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 4), Is.EqualTo(60m));
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 5), Is.EqualTo(65m));
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 6), Is.EqualTo(70m));
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 7), Is.EqualTo(75m));
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 8), Is.EqualTo(80m));
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 9), Is.EqualTo(85m));
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 10), Is.EqualTo(90m));
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 11), Is.EqualTo(95m));
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 12), Is.EqualTo(100m));

			// If somehow the game lets you go past your turn, can't go above the damage or healing that was requested
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 13), Is.EqualTo(100m));
			Assert.That(character.ChangeDamageOrHealingForStamina(100, 14), Is.EqualTo(100m));
		}

		private Character CreateTestCharacter()
		{
			var characterClass = new CharacterClass { Id = "apprentice", Cards = new List<string> { "1" } };
			return new Character(Guid.NewGuid(), CharacterColour.Blue, characterClass);
		}

		private class TestRandomWrapper : IRandomWrapper
		{
			public double Double { get; set; }

			public int Next(int minValue, int maxValue)
			{
				throw new NotImplementedException();
			}

			public int Next(int maxValue)
			{
				throw new NotImplementedException();
			}

			public double NextDouble()
			{
				return Double;
			}
		}
	}
}
