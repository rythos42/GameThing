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
		public void ApplyDamage_AccountsForDefense()
		{
			var character = CreateTestCharacter();

			character.CurrentHealth = 20;
			character.DefenseMultiplier = 2;
			character.ApplyDamage(10);
			Assert.That(character.CurrentHealth, Is.EqualTo(11));

			character.CurrentHealth = 20;
			character.DefenseMultiplier = 3;
			character.ApplyDamage(10);
			Assert.That(character.CurrentHealth, Is.EqualTo(12));

			character.CurrentHealth = 20;
			character.DefenseMultiplier = 4;
			character.ApplyDamage(10);
			Assert.That(character.CurrentHealth, Is.EqualTo(13));

			character.CurrentHealth = 20;
			character.DefenseMultiplier = 5;
			character.ApplyDamage(10);
			Assert.That(character.CurrentHealth, Is.EqualTo(14));

			character.CurrentHealth = 20;
			character.DefenseMultiplier = 6;
			character.ApplyDamage(10);
			Assert.That(character.CurrentHealth, Is.EqualTo(15));
		}

		private Character CreateTestCharacter()
		{
			var characterClass = new CharacterClass { Id = "apprentice", StartingCards = new List<string> { "1" } };
			return new Character(Guid.NewGuid(), CharacterColour.Blue, characterClass);
		}

	}
}
