using System.Runtime.Serialization;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public abstract class Condition
	{
		public virtual void ApplyImmediately(Character target) { }
		public virtual void ApplyBeforeDamage(Character source, Character target) { }
		public virtual void Remove(Character from) { }

		public Character SourceCharacter { get; set; }

		[DataMember]
		public int TurnCount { get; set; } = -1;

		[DataMember]
		public ConditionEndsOn EndsOn { get; protected set; }
	}
}
