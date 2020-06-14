using System;
using GameThing.Contract;
using GameThing.Entities;
using NUnit.Framework;

namespace GameThing.Tests.Contract
{
	[TestFixture]
	public class BattleDataTests
	{
		[Test]
		public void AnyCharacterUnactivated_ChangesWhenCharactersChangeActivation()
		{
			var battleData = new BattleData();
			var character = new Character(Guid.NewGuid(), CharacterColour.None, null);
			battleData.Characters.Add(character);

			character.ResetTurn();  // set unactivated
			Assert.That(battleData.AnyCharacterUnactivated, Is.True);

			character.EndTurn(0);    // set activated
			Assert.That(battleData.AnyCharacterUnactivated, Is.False);
		}
	}
}
