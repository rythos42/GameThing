using System.Runtime.Serialization;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public class DistractCondition : Condition
	{
		public DistractCondition(int id, string text, AbilityScore abilityScore, decimal buffPercent) : base(id, text)
		{
			AbilityScore = abilityScore;
			BuffPercent = buffPercent;
			EndsOn = ConditionEndsOn.AfterAttack;
			IconName = "sprites/icons/distract";
		}

		[DataMember]
		public AbilityScore AbilityScore { get; private set; }

		[DataMember]
		public decimal BuffPercent { get; private set; }

		public override void ApplyBeforeDamage(Character source, Character target)
		{
			if (source == SourceCharacter)
				return;

			switch (AbilityScore)
			{
				case AbilityScore.Agility: source.AgilityMultiplier *= BuffPercent; break;
				case AbilityScore.Intelligence: source.IntelligenceMultiplier *= BuffPercent; break;
				case AbilityScore.Strength: source.StrengthMultiplier *= BuffPercent; break;
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
