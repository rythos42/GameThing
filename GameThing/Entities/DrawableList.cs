using System.Collections.Generic;
using System.Linq;

namespace GameThing.Entities
{
	public class DrawableList
	{
		private readonly List<IDrawable> drawables = new List<IDrawable>();
		private List<Character> characters;

		public void Add(IDrawable drawable)
		{
			drawables.Add(drawable);
		}

		public void SetCharacters(List<Character> characters)
		{
			this.characters = characters;
		}

		public IEnumerable<IDrawable> Drawables
		{
			get
			{
				return drawables
					.Concat(characters)
					.OrderBy(drawable => drawable.DrawOrder);
			}
		}

		public bool IsTerrainAtPoint(MapPoint mapPosition)
		{
			return Drawables.Any(drawable => drawable is Terrain && drawable.IsAtPoint(mapPosition));
		}

		public bool IsEntityAtPoint(MapPoint mapPosition)
		{
			return Drawables.Any(drawable => drawable.IsAtPoint(mapPosition));
		}
	}
}
