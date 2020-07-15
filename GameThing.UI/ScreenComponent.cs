using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class ScreenComponent : UIContainer
	{
		protected override void LoadComponentContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
		{
			Dimensions = new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
			base.LoadComponentContent(contentManager, graphicsDevice);
		}
	}
}
