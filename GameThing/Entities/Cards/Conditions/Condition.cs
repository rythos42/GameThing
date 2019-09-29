using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public abstract class Condition
	{
		public Condition(int id)
		{
			Id = id;
		}

		public virtual void ApplyImmediately(Character target) { }
		public virtual void ApplyBeforeDamage(Character source, Character target) { }
		public virtual void Remove(Character from) { }

		public Character SourceCharacter { get; set; }

		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public int TurnCount { get; set; } = -1;

		[DataMember]
		public ConditionEndsOn EndsOn { get; protected set; }

		[DataMember]
		public string IconName { get; protected set; }
	}
}
