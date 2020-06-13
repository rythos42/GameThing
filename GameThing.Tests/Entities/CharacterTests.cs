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
			character.EvadeMultiplier = 0;  // Ignore Evade

			character.CurrentHealth = 20;
			character.DefenseMultiplier = 2;
			character.AttemptToApplyDamage(10);
			Assert.That(character.CurrentHealth, Is.EqualTo(11));

			character.CurrentHealth = 20;
			character.DefenseMultiplier = 3;
			character.AttemptToApplyDamage(10);
			Assert.That(character.CurrentHealth, Is.EqualTo(12));

			character.CurrentHealth = 20;
			character.DefenseMultiplier = 4;
			character.AttemptToApplyDamage(10);
			Assert.That(character.CurrentHealth, Is.EqualTo(13));

			character.CurrentHealth = 20;
			character.DefenseMultiplier = 5;
			character.AttemptToApplyDamage(10);
			Assert.That(character.CurrentHealth, Is.EqualTo(14));

			character.CurrentHealth = 20;
			character.DefenseMultiplier = 6;
			character.AttemptToApplyDamage(10);
			Assert.That(character.CurrentHealth, Is.EqualTo(15));
		}

		[Test]
		public void AttemptToApplyDamage_AccountsForEvade()
		{
			var character = CreateTestCharacter();
			character.DefenseMultiplier = 1;    // Ensure no defense

			var testRandom = new TestRandomWrapper();
			Character.Random = testRandom;

			// Evade 1 is 2.5% chance of miss
			character.CurrentHealth = 20;
			testRandom.Double = 0.025d;   // 0.025 roll > 0.025 evade is false, so no damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.CurrentHealth, Is.EqualTo(20));

			character.CurrentHealth = 20;
			testRandom.Double = 0.0251d;   // 0.0251 roll > 0.025 evade is true, so damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.CurrentHealth, Is.EqualTo(0));

			// Evade 2 is 10% chance of msis
			character.CurrentHealth = 20;
			character.EvadeMultiplier = 2;
			testRandom.Double = 0.1d;   // 0.1 roll > 0.1 evade is false, so no damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.CurrentHealth, Is.EqualTo(20));

			character.CurrentHealth = 20;
			testRandom.Double = 0.11d;   // 0.11 roll > 0.1 evade is true, so damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.CurrentHealth, Is.EqualTo(0));

			// Evade 3 is 22.5% chance of msis
			character.CurrentHealth = 20;
			character.EvadeMultiplier = 3;
			testRandom.Double = 0.225d;   // 0.225 roll > 0.225 evade is false, so no damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.CurrentHealth, Is.EqualTo(20));

			character.CurrentHealth = 20;
			testRandom.Double = 0.2251d;   // 0.2251 roll > 0.225 evade is true, so damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.CurrentHealth, Is.EqualTo(0));

			// Evade 4 is 40% chance of msis
			character.CurrentHealth = 20;
			character.EvadeMultiplier = 4;
			testRandom.Double = 0.4d;   // 0.4 roll > 0.4 evade is false, so no damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.CurrentHealth, Is.EqualTo(20));

			character.CurrentHealth = 20;
			testRandom.Double = 41;   // 0.41 roll > 0.4 evade is true, so damage
			character.AttemptToApplyDamage(20);
			Assert.That(character.CurrentHealth, Is.EqualTo(0));
		}

		private Character CreateTestCharacter()
		{
			var characterClass = new CharacterClass { Id = "apprentice", StartingCards = new List<string> { "1" } };
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
