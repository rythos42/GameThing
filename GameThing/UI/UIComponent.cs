using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public abstract class UIComponent
	{
		protected const int BOX_SHADOW_X = 5;
		protected const int BOX_SHADOW_Y = 5;
		protected const int MARGIN_X = 20;
		protected const int MARGIN_Y = 20;

		public bool IsVisible { get; set; }
		public int X { get; set; }
		public int Y { get; set; }

		public abstract void LoadContent(ContentManager content, GraphicsDevice graphicsDevice);
		public abstract void Update(GameTime gameTime);

		public virtual void Draw(SpriteBatch spriteBatch)
		{
			IsVisible = true;
		}
	}
}
