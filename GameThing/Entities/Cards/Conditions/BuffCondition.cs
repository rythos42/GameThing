using System.Runtime.Serialization;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public class BuffCondition : Condition
	{
		public BuffCondition(AbilityScore abilityScore, decimal buffPercent, int turnCount)
		{
			AbilityScore = abilityScore;
			BuffPercent = buffPercent;
			TurnCount = turnCount;
			EndsOn = ConditionEndsOn.StartRound;
		}

		[DataMember]
		public AbilityScore AbilityScore { get; private set; }

		[DataMember]
		public decimal BuffPercent { get; private set; }

		public override void ApplyImmediately(Character target)
		{
			switch (AbilityScore)
			{
				case AbilityScore.Agility: target.AgilityMultiplier *= BuffPercent; break;
				case AbilityScore.Intelligence: target.IntelligenceMultiplier *= BuffPercent; break;
				case AbilityScore.Strength: target.StrengthMultiplier *= BuffPercent; break;
			}
		}

		public override void Remove(Character from)
		{
			switch (AbilityScore)
			{
				case AbilityScore.Agility: from.AgilityMultiplier /= BuffPercent; break;
				case AbilityScore.Intelligence: from.IntelligenceMultiplier /= BuffPercent; break;
				case AbilityScore.Strength: from.StrengthMultiplier /= BuffPercent; break;
			}
		}
	}
}
