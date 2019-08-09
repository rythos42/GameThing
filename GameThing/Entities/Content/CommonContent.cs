using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities.Content
{
	public class CommonContent
	{
		public CommonContent(ContentManager content)
		{
			DistanceOverlay = content.Load<Texture2D>("sprites/distance_overlay");
		}

		public Texture2D DistanceOverlay { get; private set; }
	}
}
