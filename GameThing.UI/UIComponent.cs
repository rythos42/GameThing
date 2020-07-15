using System.Xml.Serialization;
using GameThing.UI.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public abstract class UIComponent
	{
		protected const int BOX_SHADOW_X = 5;
		protected const int BOX_SHADOW_Y = 5;

		[XmlIgnore]
		public UIComponentTappedEventHandler OnTapped { get; set; }

		[XmlIgnore]
		public UIComponentHeldEventHandler OnHeld { get; set; }

		[XmlIgnore]
		public UIComponentGestureReadEventHandler OnGestureRead { get; set; }

		[XmlIgnore]
		public ContentLoadedEventHandler OnContentLoaded { get; set; }

		[XmlAttribute]
		[XmlEvent]
		public string Tapped { get; set; }

		[XmlAttribute]
		public string Id { get; set; }

		[XmlAttribute]
		public bool IsVisible { get; set; } = true;

		[XmlAttribute]
		public int Width { get; set; }

		[XmlAttribute]
		public int Height { get; set; }

		[XmlAttribute]
		public float X { get; set; }

		[XmlAttribute]
		public float Y { get; set; }

		[XmlAttribute]
		public int Padding { get; set; } = 8;

		[XmlAttribute]
		public int Margin { get; set; } = 8;

		[XmlAttribute]
		public bool Enabled { get; set; } = true;

		[XmlIgnore]
		public bool HasContentLoaded { get; protected set; }

		[XmlIgnore]
		public Vector2 Position { get => new Vector2(X, Y); set { X = value.X; Y = value.Y; } }

		[XmlIgnore]
		public Vector2 Dimensions { get => new Vector2(Width, Height); set { Width = (int) value.X; Height = (int) value.Y; } }

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

		public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
		{
			LoadComponentContent(content, graphicsDevice);
			HasContentLoaded = true;
			OnContentLoaded?.Invoke();
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			if (IsVisible)
				DrawComponent(spriteBatch);
		}

		public void Draw(SpriteBatch spriteBatch, float x, float y)
		{
			var oldX = X;
			var oldY = Y;
			X = x;
			Y = y;
			Draw(spriteBatch);
			X = oldX;
			Y = oldY;
		}

		protected abstract void LoadComponentContent(ContentManager content, GraphicsDevice graphicsDevice);
		protected abstract void DrawComponent(SpriteBatch spriteBatch);
	}
}
