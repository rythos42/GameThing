using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class ScreenComponent : UIContainer
	{
		private readonly Vector2 screenSize;

		public ScreenComponent(float width, float height)
		{
			screenSize = new Vector2(width, height);
		}

		public override void Draw(SpriteBatch spriteBatch, float x, float y)
		{
			// Not using this to draw the screen yet.
			throw new NotImplementedException();
		}

		public override Vector2 MeasureContent()
		{
			return screenSize;
		}
	}
}
