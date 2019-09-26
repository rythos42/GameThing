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

		public override void Play(int roundNumber, Character target = null)
		{
			target.Conditions.Add(new AppliedCondition(Condition, roundNumber));
			Condition.ApplyImmediately(target);
		}

		[DataMember]
		public Condition Condition { get; private set; }
	}
}
