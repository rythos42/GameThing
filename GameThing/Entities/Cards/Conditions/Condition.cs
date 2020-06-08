using System;
using System.Runtime.Serialization;
using GameThing.Contract;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public class Condition
	{
		public void ApplyImmediately(Character target)
		{
			var random = (decimal) new Random().NextDouble();
			if (random > SuccessPercent)
				return;

			switch (Type)
			{
				case ConditionType.Buff:
					switch (AbilityScore.Value)
					{
						case Contract.AbilityScore.Health: target.CurrentHealth = ApplyBuff(target.CurrentHealth, BuffAmount.Value); break;
						case Contract.AbilityScore.Agility: target.AgilityMultiplier = ApplyBuff(target.AgilityMultiplier, BuffAmount.Value); break;
						case Contract.AbilityScore.Intelligence: target.IntelligenceMultiplier = ApplyBuff(target.IntelligenceMultiplier, BuffAmount.Value); break;
						case Contract.AbilityScore.Strength: target.StrengthMultiplier = ApplyBuff(target.StrengthMultiplier, BuffAmount.Value); break;
					}
					break;

				case ConditionType.Taunt:
					target.NextCardMustTarget = SourceCharacter;
					break;

				case ConditionType.Run:
					target.RemainingMoves = (int) Math.Round(ApplyBuff(target.RemainingMoves, BuffAmount.Value));
					target.MaximumMoves += (int) Math.Round(ApplyBuff(target.MaximumMoves, BuffAmount.Value));
					break;
			}
		}

		public void ApplyBeforeDamage(Character source, Character target)
		{
			var random = (decimal) new Random().NextDouble();
			if (random > SuccessPercent)
				return;

			switch (Type)
			{
				case ConditionType.Distract:
					if (source == SourceCharacter)
						return;

					switch (AbilityScore)
					{
						case Contract.AbilityScore.Agility: source.AgilityMultiplier = ApplyBuff(source.AgilityMultiplier, BuffAmount.Value); break;
						case Contract.AbilityScore.Intelligence: source.IntelligenceMultiplier = ApplyBuff(source.IntelligenceMultiplier, BuffAmount.Value); break;
						case Contract.AbilityScore.Strength: source.StrengthMultiplier = ApplyBuff(source.StrengthMultiplier, BuffAmount.Value); break;
					}
					break;
			}
		}

		public void Remove(Character from)
		{
			switch (Type)
			{
				case ConditionType.Buff:
					switch (AbilityScore)
					{
						case Contract.AbilityScore.Health: from.CurrentHealth = RemoveBuff(from.CurrentHealth, BuffAmount.Value); break;
						case Contract.AbilityScore.Agility: from.AgilityMultiplier = RemoveBuff(from.AgilityMultiplier, BuffAmount.Value); break;
						case Contract.AbilityScore.Intelligence: from.IntelligenceMultiplier = RemoveBuff(from.IntelligenceMultiplier, BuffAmount.Value); break;
						case Contract.AbilityScore.Strength: from.StrengthMultiplier = RemoveBuff(from.StrengthMultiplier, BuffAmount.Value); break;
					}
					break;

				case ConditionType.Run:
					from.MaximumMoves = (int) Math.Round(RemoveBuff(from.MaximumMoves, BuffAmount.Value));
					break;

				case ConditionType.Distract:
					switch (AbilityScore)
					{
						case Contract.AbilityScore.Agility: from.AgilityMultiplier = RemoveBuff(from.AgilityMultiplier, BuffAmount.Value); break;
						case Contract.AbilityScore.Intelligence: from.IntelligenceMultiplier = RemoveBuff(from.IntelligenceMultiplier, BuffAmount.Value); break;
						case Contract.AbilityScore.Strength: from.StrengthMultiplier = RemoveBuff(from.StrengthMultiplier, BuffAmount.Value); break;
					}
					break;
			}
		}

		private decimal ApplyBuff(decimal current, decimal amount)
		{
			switch (BuffType.Value)
			{
				case Conditions.BuffType.Linear:
					return current + amount;
				case Conditions.BuffType.Percent:
					return current * amount;
				default:
					throw new Exception($"Bad BuffType {BuffType}.");
			}
		}

		private decimal RemoveBuff(decimal current, decimal amount)
		{
			switch (BuffType.Value)
			{
				case Conditions.BuffType.Linear:
					return current - amount;
				case Conditions.BuffType.Percent:
					return current / amount;
				default:
					throw new Exception($"Bad BuffType {BuffType}.");
			}
		}

		public Character SourceCharacter { get; set; }

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
		public ConditionType Type { get; set; }

		[DataMember]
		public AbilityScore? AbilityScore { get; set; }

		[DataMember]
		public decimal? BuffAmount { get; set; }

		[DataMember]
		public BuffType? BuffType { get; set; }

		[DataMember]
		public decimal SuccessPercent { get; set; }
	}
}
