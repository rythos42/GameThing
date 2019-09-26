using System;
using System.Runtime.Serialization;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public class BuffCondition : Condition
	{
		private readonly Func<decimal, decimal, decimal> applyBuff;
		private readonly Func<decimal, decimal, decimal> removeBuff;

		public BuffCondition(int id, AbilityScore abilityScore, decimal buffAmount, int turnCount, BuffType type) : base(id)
		{
			AbilityScore = abilityScore;
			BuffAmount = buffAmount;
			TurnCount = turnCount;
			EndsOn = ConditionEndsOn.StartRound;

			switch (type)
			{
				case BuffType.Linear:
					applyBuff = (current, amount) => current + amount;
					removeBuff = (current, amount) => current - amount;
					break;
				case BuffType.Percent:
					applyBuff = (current, amount) => current * amount;
					removeBuff = (current, amount) => current / amount;
					break;
			}

		}

		[DataMember]
		public AbilityScore AbilityScore { get; private set; }

		[DataMember]
		public decimal BuffAmount { get; private set; }

		public override void ApplyImmediately(Character target)
		{
			switch (AbilityScore)
			{
				case AbilityScore.Health: target.CurrentHealth = applyBuff(target.CurrentHealth, BuffAmount); break;
				case AbilityScore.Agility: target.AgilityMultiplier = applyBuff(target.AgilityMultiplier, BuffAmount); break;
				case AbilityScore.Intelligence: target.IntelligenceMultiplier = applyBuff(target.IntelligenceMultiplier, BuffAmount); break;
				case AbilityScore.Strength: target.StrengthMultiplier = applyBuff(target.StrengthMultiplier, BuffAmount); break;
			}
		}

		public override void Remove(Character from)
		{
			switch (AbilityScore)
			{
				case AbilityScore.Health: from.CurrentHealth = removeBuff(from.CurrentHealth, BuffAmount); break;
				case AbilityScore.Agility: from.AgilityMultiplier = removeBuff(from.AgilityMultiplier, BuffAmount); break;
				case AbilityScore.Intelligence: from.IntelligenceMultiplier = removeBuff(from.IntelligenceMultiplier, BuffAmount); break;
				case AbilityScore.Strength: from.StrengthMultiplier = removeBuff(from.StrengthMultiplier, BuffAmount); break;
			}
		}
	}
}
