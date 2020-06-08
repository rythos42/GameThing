using GameThing.Database;

namespace GameThing.Entities.Cards
{
	public class Category : IIdentifiable
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}
}
