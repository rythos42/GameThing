using System;
using System.Runtime.Serialization;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public class Condition
	{
		public Condition(int id, string text, ConditionType type)
		{
			Id = id;
			Text = text;
			Type = type;

			switch (type)
			{
				case ConditionType.Buff:
					EndsOn = ConditionEndsOn.StartRound;
					IconName = "sprites/icons/buff";
					break;
				case ConditionType.Taunt:
					EndsOn = ConditionEndsOn.AfterAttack;
					IconName = "sprites/icons/taunt";
					break;
				case ConditionType.Run:
					EndsOn = ConditionEndsOn.Move;
					IconName = "sprites/icons/run";
					break;
				case ConditionType.Distract:
					EndsOn = ConditionEndsOn.AfterAttack;
					IconName = "sprites/icons/distract";
					break;
			}
		}

		public void ApplyImmediately(Character target)
		{
			switch (Type)
			{
				case ConditionType.Buff:
					switch (AbilityScore)
					{
						case AbilityScore.Health: target.CurrentHealth = ApplyBuff(target.CurrentHealth, BuffAmount); break;
						case AbilityScore.Agility: target.AgilityMultiplier = ApplyBuff(target.AgilityMultiplier, BuffAmount); break;
						case AbilityScore.Intelligence: target.IntelligenceMultiplier = ApplyBuff(target.IntelligenceMultiplier, BuffAmount); break;
						case AbilityScore.Strength: target.StrengthMultiplier = ApplyBuff(target.StrengthMultiplier, BuffAmount); break;
					}
					break;

				case ConditionType.Taunt:
					var random = (decimal) new Random().NextDouble();
					if (random > SuccessPercent)
						return;

					target.NextCardMustTarget = SourceCharacter;
					break;

				case ConditionType.Run:
					target.RemainingMoves += (int) BuffAmount;
					target.MaximumMoves += (int) BuffAmount;
					break;
			}
		}

		public void ApplyBeforeDamage(Character source, Character target)
		{
			switch (Type)
			{
				case ConditionType.Distract:
					if (source == SourceCharacter)
						return;

					switch (AbilityScore)
					{
						case AbilityScore.Agility: source.AgilityMultiplier *= BuffAmount; break;
						case AbilityScore.Intelligence: source.IntelligenceMultiplier *= BuffAmount; break;
						case AbilityScore.Strength: source.StrengthMultiplier *= BuffAmount; break;
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
						case AbilityScore.Health: from.CurrentHealth = RemoveBuff(from.CurrentHealth, BuffAmount); break;
						case AbilityScore.Agility: from.AgilityMultiplier = RemoveBuff(from.AgilityMultiplier, BuffAmount); break;
						case AbilityScore.Intelligence: from.IntelligenceMultiplier = RemoveBuff(from.IntelligenceMultiplier, BuffAmount); break;
						case AbilityScore.Strength: from.StrengthMultiplier = RemoveBuff(from.StrengthMultiplier, BuffAmount); break;
					}
					break;

				case ConditionType.Run:
					from.MaximumMoves -= (int) BuffAmount;
					break;

				case ConditionType.Distract:
					switch (AbilityScore)
					{
						case AbilityScore.Agility: from.AgilityMultiplier /= BuffAmount; break;
						case AbilityScore.Intelligence: from.IntelligenceMultiplier /= BuffAmount; break;
						case AbilityScore.Strength: from.StrengthMultiplier /= BuffAmount; break;
					}
					break;
			}
		}

		private decimal ApplyBuff(decimal current, decimal amount)
		{
			switch (BuffType)
			{
				case BuffType.Linear:
					return current + amount;
				case BuffType.Percent:
					return current * amount;
				default:
					throw new Exception($"Bad BuffType {BuffType}.");
			}
		}

		private decimal RemoveBuff(decimal current, decimal amount)
		{
			switch (BuffType)
			{
				case BuffType.Linear:
					return current - amount;
				case BuffType.Percent:
					return current / amount;
				default:
					throw new Exception($"Bad BuffType {BuffType}.");
			}
		}

		public Character SourceCharacter { get; set; }

		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Text { get; set; }

		[DataMember]
		public int TurnCount { get; set; } = -1;

		[DataMember]
		public ConditionEndsOn EndsOn { get; protected set; }

		[DataMember]
		public string IconName { get; protected set; }

		[DataMember]
		public ConditionType Type { get; set; }

		[DataMember]
		public AbilityScore AbilityScore { get; set; }

		[DataMember]
		public decimal BuffAmount { get; set; }

		[DataMember]
		public BuffType BuffType { get; set; }

		[DataMember]
		public decimal SuccessPercent { get; set; }
	}
}
