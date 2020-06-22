using System;
using GameThing.Contract;
using GameThing.Entities;
using GameThing.Entities.Cards.Conditions;
using NUnit.Framework;

namespace GameThing.Tests.Entities.Cards.Conditions
{
	[TestFixture]
	public class EffectTests
	{
		[Test]
		public void Apply_Applies_Buff_Linear()
		{
			var target = new Character(Guid.NewGuid(), CharacterColour.Blue, null);
			var effect = new Effect
			{
				Type = EffectType.Buff,
				AbilityScore = AbilityScore.Strength,
				BuffAmount = 2,
				BuffType = BuffType.Linear
			};

			effect.Apply(null, target, null);

			Assert.That(target.GetCurrentAbilityScore(AbilityScore.Strength), Is.EqualTo(3));
		}

		[Test]
		public void Apply_Applies_Buff_Percent()
		{
			var target = new Character(Guid.NewGuid(), CharacterColour.Blue, null);
			var effect = new Effect
			{
				Type = EffectType.Buff,
				AbilityScore = AbilityScore.Strength,
				BuffAmount = 2,
				BuffType = BuffType.Percent
			};

			effect.Apply(null, target, null);

			Assert.That(target.GetCurrentAbilityScore(AbilityScore.Strength), Is.EqualTo(2));
		}

		[Test]
		public void Apply_Applies_Taunt()
		{
			var target = new Character(Guid.NewGuid(), CharacterColour.Blue, null);
			var source = new Character(Guid.NewGuid(), CharacterColour.Blue, null);
			var effect = new Effect
			{
				Type = EffectType.Taunt
			};

			effect.Apply(source, target, null);

			Assert.That(target.NextCardMustTarget.Id, Is.EqualTo(source.Id));
		}

		[Test]
		public void Apply_Applies_Run_Linear()
		{
			var target = new Character(Guid.NewGuid(), CharacterColour.Blue, null);
			var effect = new Effect
			{
				Type = EffectType.Run,
				BuffAmount = 2,
				BuffType = BuffType.Linear
			};

			effect.Apply(null, target, null);

			Assert.That(target.RemainingMoves, Is.EqualTo(2));  // starts at 0
			Assert.That(target.MaximumMoves, Is.EqualTo(7));    // starts at 5
		}

		[Test]
		public void Apply_Applies_Run_Percent()
		{
			var target = new Character(Guid.NewGuid(), CharacterColour.Blue, null) { RemainingMoves = 1 };
			var effect = new Effect
			{
				Type = EffectType.Run,
				BuffAmount = 2,
				BuffType = BuffType.Percent
			};

			effect.Apply(null, target, null);

			Assert.That(target.RemainingMoves, Is.EqualTo(2));  // starts at 0, set to 1 in construction
			Assert.That(target.MaximumMoves, Is.EqualTo(10));   // starts at 5
		}

		[Test]
		public void Apply_Applies_Distract_Linear_ToSource()
		{
			var target = new Character(Guid.NewGuid(), CharacterColour.Blue, null);
			var source = new Character(Guid.NewGuid(), CharacterColour.Blue, null);
			var effect = new Effect
			{
				Type = EffectType.Distract,
				AbilityScore = AbilityScore.Strength,
				BuffAmount = 2,
				BuffType = BuffType.Linear
			};

			effect.Apply(source, target, null);

			Assert.That(source.GetCurrentAbilityScore(AbilityScore.Strength), Is.EqualTo(3));
		}

		[Test]
		public void Apply_Applies_Distract_Percent_ToSource()
		{
			var target = new Character(Guid.NewGuid(), CharacterColour.Blue, null);
			var source = new Character(Guid.NewGuid(), CharacterColour.Blue, null);
			var effect = new Effect
			{
				Type = EffectType.Distract,
				AbilityScore = AbilityScore.Strength,
				BuffAmount = 2,
				BuffType = BuffType.Percent
			};

			effect.Apply(source, target, null);

			Assert.That(source.GetCurrentAbilityScore(AbilityScore.Strength), Is.EqualTo(2));
		}

		[Test]
		public void Apply_DoesNot_Apply_Distract_IfSourceEqualsOwner()
		{
			var target = new Character(Guid.NewGuid(), CharacterColour.Blue, null);
			var source = new Character(Guid.NewGuid(), CharacterColour.Blue, null);
			var effect = new Effect
			{
				Type = EffectType.Distract,
				AbilityScore = AbilityScore.Strength,
				BuffAmount = 2,
				BuffType = BuffType.Percent
			};

			effect.Apply(source, target, source);

			Assert.That(source.GetCurrentAbilityScore(AbilityScore.Strength), Is.EqualTo(1));
		}

		[Test]
		public void Apply_Applies_Remove_Health()
		{
			var target = new Character(Guid.NewGuid(), CharacterColour.Blue, null);
			var effect = new Effect
			{
				Type = EffectType.Remove,
				BuffAmount = 1,
			};

			effect.Apply(null, target, null);

			Assert.That(target.GetCurrentAbilityScore(AbilityScore.Health), Is.EqualTo(6));
		}
	}
}
