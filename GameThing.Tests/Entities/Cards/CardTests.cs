using System;
using GameThing.Contract;
using GameThing.Entities;
using GameThing.Entities.Cards;
using NUnit.Framework;

namespace GameThing.Tests.Entities.Cards
{
	[TestFixture]
	public class CardTests
	{
		[Test]
		public void FullDescription_UsesTemplate()
		{
			var character = new Character(Guid.NewGuid(), CharacterColour.Blue, new CharacterClass());
			var card = new Card()
			{
				Description = "Deal 100% Str (<character.CurrentStrength>) damage at range 1.",
				OwnerCharacter = character
			};

			Assert.That(card.FullDescription, Is.EqualTo("Deal 100% Str (1) damage at range 1."));
		}
	}
}
