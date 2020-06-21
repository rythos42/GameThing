using System;
using System.Runtime.Serialization;
using GameThing.Contract;

namespace GameThing.Entities.Cards.Conditions
{
	[DataContract]
	public class Effect
	{
		[DataMember]
		public EffectType Type { get; set; }

		[DataMember]
		public AbilityScore? AbilityScore { get; set; }

		[DataMember]
		public decimal? BuffAmount { get; set; }

		[DataMember]
		public BuffType? BuffType { get; set; }

		private AbilityScore Score { get { return AbilityScore.Value; } }

		public void Apply(Character source, Character target, Character owner)
		{
			switch (Type)
			{
				case EffectType.Buff:
					target.SetAbilityScoreMultiplier(Score, ApplyBuff(target.GetAbilityScoreMultiplier(Score), BuffAmount.Value));
					break;

				case EffectType.Taunt:
					target.NextCardMustTarget = source;
					break;

				case EffectType.Run:
					target.RemainingMoves = (int) Math.Round(ApplyBuff(target.RemainingMoves, BuffAmount.Value));
					target.MaximumMoves += (int) Math.Round(ApplyBuff(target.MaximumMoves, BuffAmount.Value));
					break;

				case EffectType.Distract:
					if (source == owner)
						return;

					source.SetAbilityScoreMultiplier(Score, ApplyBuff(source.GetAbilityScoreMultiplier(Score), BuffAmount.Value));
					break;
			}
		}

		public void Remove(Character from)
		{
			switch (Type)
			{
				case EffectType.Buff:
				case EffectType.Distract:
					var score = AbilityScore.Value;
					from.SetAbilityScoreMultiplier(score, RemoveBuff(from.GetAbilityScoreMultiplier(score), BuffAmount.Value));
					break;

				case EffectType.Run:
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
	}
}
