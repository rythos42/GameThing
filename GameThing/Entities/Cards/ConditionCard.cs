using System.Linq;
using System.Runtime.Serialization;
using GameThing.Entities.Cards.Conditions;

namespace GameThing.Entities.Cards
{
	[DataContract]
	public class ConditionCard : Card
	{
		public ConditionCard(int id, string title, string description, int range, Character ownerCharacter, Condition condition)
			: base(id, title, description, range, ownerCharacter)
		{
			Condition = condition;
			condition.SourceCharacter = ownerCharacter;
		}

		public override bool Play(int roundNumber, Character target = null)
		{
			// if the condition is on the target already, cancel the card play
			if (target.Conditions.Any(applied => applied.Condition.Id == Condition.Id))
				return false;

			target.Conditions.Add(new AppliedCondition(Condition, roundNumber));
			Condition.ApplyImmediately(target);

			return true;
		}

		[DataMember]
		public Condition Condition { get; private set; }
	}
}
