using System.Collections.Generic;

namespace GameThing.Entities
{
	public class CharacterDepthComparer : IComparer<Character>
	{
		public int Compare(Character one, Character two)
		{
			return one.MapPosition.GetScreenPosition().Y.CompareTo(two.MapPosition.GetScreenPosition().Y);
		}
	}
}
