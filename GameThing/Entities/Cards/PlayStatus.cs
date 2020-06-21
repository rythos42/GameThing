using GameThing.Entities.Cards.Requirements;

namespace GameThing.Entities.Cards
{
	public enum PlayStatusDetails
	{
		Success,
		FailedTaunted,
		FailedNoStack,
		FailedEvaded,
		FailedRequirement
	}

	public class PlayStatus
	{
		public PlayStatus(PlayStatusDetails status)
		{
			Status = status;
		}

		public PlayStatusDetails Status { get; set; }
		public bool PlayCancelled { get; set; }
		public decimal ActualDamageOrHealingDone { get; set; }
		public CardType CardType { get; set; }
		public RequirementType RequirementType { get; set; }
	}
}
