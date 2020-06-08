using System.Collections.Generic;
using GameThing.Database;

namespace GameThing.Entities
{
	public class CharacterClass : IIdentifiable
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public IList<string> StartingCards { get; set; }
	}
}
