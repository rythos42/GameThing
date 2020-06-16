using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public class Condition
	{
		public void ApplyEffects(Character source, Character target)
		{
			var random = (decimal) new Random().NextDouble();
			if (random > SuccessPercent)
				return;

			Effects.ForEach(effect => effect.Apply(source, target, OwningCharacter));
		}

		public void RemoveEffects(Character from)
		{
			Effects.ForEach(effect => effect.Remove(from));
		}

		public Character OwningCharacter { get; set; }

		[DataMember]
		public string StackGroup { get; set; }

		[DataMember]
		public string Text { get; set; }

		[DataMember]
		public int TurnCount { get; set; } = -1;

		[DataMember]
		public ConditionEndsOn EndsOn { get; set; }

		[DataMember]
		public string IconName { get; set; }

		[DataMember]
		public decimal SuccessPercent { get; set; }

		[DataMember]
		public List<Effect> Effects { get; set; }
	}
}
