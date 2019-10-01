﻿using System.Runtime.Serialization;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public class RunCondition : Condition
	{
		public RunCondition(int id, string text, int additionalMove) : base(id, text)
		{
			AdditionalMove = additionalMove;
			EndsOn = ConditionEndsOn.Move;
			IconName = "sprites/icons/run";
		}

		[DataMember]
		public int AdditionalMove { get; private set; }

		public override void ApplyImmediately(Character target)
		{
			target.RemainingMoves += AdditionalMove;
			target.MaximumMoves += AdditionalMove;
		}

		public override void Remove(Character from)
		{
			from.MaximumMoves -= AdditionalMove;
		}
	}
}
