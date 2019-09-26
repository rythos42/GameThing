using System.Runtime.Serialization;
using GameThing.Entities.Cards.Conditions;

namespace GameThing.Entities.Cards
{
	[DataContract]
	public class DamageCard : Card
	{
		public DamageCard(
			int id,
			string title,
			string description,
			int range,
			Character ownerCharacter,
			AbilityScore abilityScore,
			decimal damagePercent)
			: base(id, title, description, range, ownerCharacter)
		{
			AbilityScore = abilityScore;
			DamagePercent = damagePercent;
		}

		[DataMember]
		public decimal DamagePercent { get; private set; }

		[DataMember]
		public AbilityScore AbilityScore { get; private set; }

		public override void Play(int roundNumber, Character target = null)
		{
			decimal damage = 0;

			target.Conditions.ForEach(condition => condition.Condition.ApplyBeforeDamage(OwnerCharacter, target));

			switch (AbilityScore)
			{
				case AbilityScore.Agility: damage = OwnerCharacter.CurrentAgility * DamagePercent; break;
				case AbilityScore.Strength: damage = OwnerCharacter.CurrentStrength * DamagePercent; break;
				case AbilityScore.Intelligence: damage = OwnerCharacter.CurrentIntelligence * DamagePercent; break;
			}
			target.ApplyDamage(damage);

			target.RemoveConditions(ConditionEndsOn.AfterAttack);
		}
	}
}
