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
		public UIComponentHeldEventHandler Held;
		public UIComponentGestureReadEventHandler GestureRead;
		public ContentLoadedEventHandler ContentLoaded;

		public string Id { get; set; }
		public bool IsVisible { get; set; }
		public int Width { get; protected set; }
		public int Height { get; protected set; }
		public Vector2 Dimensions { get { return new Vector2(Width, Height); } set { Width = (int) value.X; Height = (int) value.Y; } }
		public float X { get; protected set; }
		public float Y { get; protected set; }
		public Vector2 Position { get { return new Vector2(X, Y); } set { X = value.X; Y = value.Y; } }
		public bool HasContentLoaded { get; protected set; }
		public bool Enabled { get; set; } = true;

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

		public void Draw(SpriteBatch spriteBatch)
		{
			Draw(spriteBatch, X, Y);
		}

		public void DrawConditional(SpriteBatch spriteBatch, float x, float y, bool draw)
		{
			if (draw)
				Draw(spriteBatch, x, y);
			else
				IsVisible = false;
		}

		public void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			LoadComponentContent(content, graphicsDevice);
			HasContentLoaded = true;
			ContentLoaded?.Invoke();
		}

		protected abstract void LoadComponentContent(Content content, GraphicsDevice graphicsDevice);
		public abstract void Draw(SpriteBatch spriteBatch, float x, float y);
		public abstract Vector2 MeasureContent();
	}
}
