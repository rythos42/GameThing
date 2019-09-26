using System.Runtime.Serialization;

namespace GameThing.Entities.Cards
{
	[DataContract]
	public class HealCard : Card
	{
		public HealCard(
			int id,
			string title,
			string description,
			int range,
			Character ownerCharacter,
			AbilityScore abilityScore,
			int healPercent)
			: base(id, title, description, range, ownerCharacter)
		{
			AbilityScore = abilityScore;
			HealPercent = healPercent;
		}

		[DataMember]
		public int HealPercent { get; private set; }

		[DataMember]
		public AbilityScore AbilityScore { get; private set; }

		public override void Play(int roundNumber, Character target = null)
		{
			decimal healing = 0;
			switch (AbilityScore)
			{
				case AbilityScore.Agility: healing = OwnerCharacter.CurrentAgility * HealPercent; break;
				case AbilityScore.Strength: healing = OwnerCharacter.CurrentStrength * HealPercent; break;
				case AbilityScore.Intelligence: healing = OwnerCharacter.CurrentIntelligence * HealPercent; break;
			}
			target.ApplyHealing(healing);
		}
	}
}
