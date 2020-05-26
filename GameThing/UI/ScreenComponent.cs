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
			spriteBatch.Draw(Background, new Rectangle(0, 0, (int) screenSize.X, (int) screenSize.Y), Color.White);
		}

		public override Vector2 MeasureContent()
		{
			return screenSize;
		}

		public Texture2D Background { get; set; }
	}
}
