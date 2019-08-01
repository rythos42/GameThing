using System;
using System.Runtime.Serialization;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public class TauntCondition : Condition
	{
		public TauntCondition(decimal successPercent)
		{
			SuccessPercent = successPercent;
			EndsOn = ConditionEndsOn.AfterAttack;
		}

		[DataMember]
		public decimal SuccessPercent { get; private set; }

		public override void ApplyImmediately(Character target)
		{
			var random = (decimal) new Random().NextDouble();
			if (random > SuccessPercent)
				return;

			target.NextCardMustTarget = SourceCharacter;
		}
	}
}
