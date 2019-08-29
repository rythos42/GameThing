using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public abstract class UIComponent
	{
		protected const int BOX_SHADOW_X = 5;
		protected const int BOX_SHADOW_Y = 5;
		protected const int MARGIN = 16;
		protected const int PADDING = 16;

		public bool IsVisible { get; protected set; }

		public virtual void Update(GameTime gameTime)
		{
		}

		public abstract void LoadContent(Content content, GraphicsDevice graphicsDevice);
		public abstract void Draw(SpriteBatch spriteBatch, int x, int y);
		public abstract Vector2 MeasureContent();
	}
}
