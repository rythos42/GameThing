using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class ScreenComponent : UIContainer
	{
		protected override void DrawComponent(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Background, new Rectangle(0, 0, (int) Dimensions.X, (int) Dimensions.Y), Color.White);
		}

		protected override void LoadComponentContent(Content content, GraphicsDevice graphicsDevice)
		{
			Dimensions = new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
			base.LoadComponentContent(content, graphicsDevice);
		}

		public Texture2D Background { get; set; }
	}
}
