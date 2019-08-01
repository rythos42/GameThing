using System.Runtime.Serialization;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public class AppliedCondition
	{
		public AppliedCondition(Condition condition, int roundNumber)
		{
			Condition = condition;
			RoundNumber = roundNumber;
		}

		[DataMember]
		public Condition Condition { get; set; }

		[DataMember]
		public int RoundNumber { get; set; }
	}
}
