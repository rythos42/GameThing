using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public abstract class UIComponent
	{
		protected const int BOX_SHADOW_X = 5;
		protected const int BOX_SHADOW_Y = 5;

		public UIComponentTappedEventHandler Tapped;
		public UIComponentHeldEventHandler Held;
		public UIComponentGestureReadEventHandler GestureRead;
		public ContentLoadedEventHandler ContentLoaded;

		public string Id { get; set; }
		public bool IsVisible { get; set; } = true;
		public int Width { get; protected set; }
		public int Height { get; protected set; }
		public Vector2 Dimensions { get => new Vector2(Width, Height); set { Width = (int) value.X; Height = (int) value.Y; } }
		public float X { get; set; }
		public float Y { get; set; }
		public int Padding { get; set; } = 8;
		public int Margin { get; set; } = 8;
		public Vector2 Position { get => new Vector2(X, Y); set { X = value.X; Y = value.Y; } }
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

		public void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			LoadComponentContent(content, graphicsDevice);
			HasContentLoaded = true;
			ContentLoaded?.Invoke();
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			if (IsVisible)
				DrawComponent(spriteBatch);
		}

		protected abstract void LoadComponentContent(Content content, GraphicsDevice graphicsDevice);
		protected abstract void DrawComponent(SpriteBatch spriteBatch);
	}
}
