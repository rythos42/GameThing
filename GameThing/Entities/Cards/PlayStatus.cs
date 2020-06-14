namespace GameThing.Entities.Cards
{
	public enum PlayStatusDetails
	{
		Success,
		FailedTaunted,
		FailedNoStack,
		FailedEvaded
	}

	public class PlayStatus
	{
		public PlayStatus(PlayStatusDetails status)
		{
			Status = status;
		}

		public PlayStatusDetails Status { get; set; }
		public bool PlayCancelled { get; set; }
	}
}
