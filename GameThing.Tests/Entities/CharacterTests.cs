using System;
using System.Collections.Generic;
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
			var characterClass = new CharacterClass { Id = "apprentice", Name = "Apprentice" };
			var character = new Character(Guid.NewGuid(), Contract.CharacterColour.Blue, characterClass);

			var characterJson = JsonConvert.SerializeObject(character);
			var characterJObj = JObject.Parse(characterJson);

			var serializedCharacterClass = characterJObj.GetValue("CharacterClass").ToString();
			Assert.That(serializedCharacterClass, Is.EqualTo("apprentice"));
		}

		[Test]
		public void Serializes_DeckAsListOfCardIds_NotEntireObject()
		{
			var characterClass = new CharacterClass { Id = "apprentice", StartingCards = new List<string> { "1" } };
			var character = new Character(Guid.NewGuid(), Contract.CharacterColour.Blue, characterClass);
			character.Deck.Add(new Card { Id = "1" });

			var characterJson = JsonConvert.SerializeObject(character);
			var characterJObj = JObject.Parse(characterJson);

			var serializedDeck = characterJObj.GetValue("Deck").ToObject<List<string>>();
			Assert.That(serializedDeck.Count, Is.EqualTo(1));
			Assert.That(serializedDeck[0], Is.EqualTo("1"));
		}
	}
}
