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
					var score = AbilityScore.Value;
					target.SetAbilityScoreMultiplier(score, ApplyBuff(target.GetAbilityScoreMultiplier(score), BuffAmount.Value));
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

					var score = AbilityScore.Value;
					source.SetAbilityScoreMultiplier(score, ApplyBuff(source.GetAbilityScoreMultiplier(score), BuffAmount.Value));
					break;
			}
		}

		public void Remove(Character from)
		{
			var score = AbilityScore.Value;
			switch (Type)
			{
				case ConditionType.Buff:
				case ConditionType.Distract:
					from.SetAbilityScoreMultiplier(score, RemoveBuff(from.GetAbilityScoreMultiplier(score), BuffAmount.Value));
					break;

				case ConditionType.Run:
					from.MaximumMoves = (int) Math.Round(RemoveBuff(from.MaximumMoves, BuffAmount.Value));
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
