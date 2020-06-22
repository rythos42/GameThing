using GameThing.Entities.Cards.Conditions.Recurrence;
using NUnit.Framework;

namespace GameThing.Tests.Entities.Cards.Conditions.Recurrence
{
	[TestFixture]
	public class RecurrenceTests
	{
		[Test]
		public void Is_Identifies_Recurrence()
		{
			var recurrence = new GameThing.Entities.Cards.Conditions.Recurrence.Recurrence { Period = RecurrencePeriod.PerRound, Trigger = RecurrenceTrigger.End };

			Assert.That(recurrence.Is(RecurrencePeriod.PerRound, RecurrenceTrigger.End), Is.True);
		}
	}
}
