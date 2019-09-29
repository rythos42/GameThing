using System;
using System.Runtime.Serialization;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public class TauntCondition : Condition
	{
		public TauntCondition(int id, decimal successPercent) : base(id)
		{
			SuccessPercent = successPercent;
			EndsOn = ConditionEndsOn.AfterAttack;
			IconName = "sprites/icons/taunt";
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
