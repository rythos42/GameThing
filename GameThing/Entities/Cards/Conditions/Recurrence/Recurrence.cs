namespace GameThing.Entities.Cards.Conditions.Recurrence
{
	public class Recurrence
	{
		public RecurrenceTrigger Trigger { get; set; }
		public RecurrencePeriod Period { get; set; }

		public bool Is(RecurrencePeriod period, RecurrenceTrigger trigger)
		{
			return period == Period && trigger == Trigger;
		}
	}
}
