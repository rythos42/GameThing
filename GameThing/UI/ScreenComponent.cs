using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class ScreenComponent : UIContainer
	{
		protected override void LoadComponentContent(Content content, GraphicsDevice graphicsDevice)
		{
			Dimensions = new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
			base.LoadComponentContent(content, graphicsDevice);
		}
	}
}
