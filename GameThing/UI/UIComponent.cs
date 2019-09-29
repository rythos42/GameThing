using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public abstract class UIComponent
	{
		protected const int BOX_SHADOW_X = 5;
		protected const int BOX_SHADOW_Y = 5;
		public const int MARGIN = 16;
		public const int PADDING = 16;

		public UIComponentTappedEventHandler Tapped;

		public bool IsVisible { get; set; }
		public int Width { get; protected set; }
		public int Height { get; protected set; }
		public float X { get; protected set; }
		public float Y { get; protected set; }

		public virtual void Update(GameTime gameTime)
		{
		}

		public bool IsAtPoint(Vector2 checkPoint)
		{
			return
				X < checkPoint.X
				&& X + Width > checkPoint.X
				&& Y < checkPoint.Y
				&& Y + Height > checkPoint.Y;
		}

		public void Draw(SpriteBatch spriteBatch, Vector2 location)
		{
			Draw(spriteBatch, location.X, location.Y);
		}

		public abstract void LoadContent(Content content, GraphicsDevice graphicsDevice);
		public abstract void Draw(SpriteBatch spriteBatch, float x, float y);
		public abstract Vector2 MeasureContent();
	}
}
