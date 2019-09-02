using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities
{
	public abstract class Drawable : IDrawable
	{
		public int DrawOrder
		{
			get
			{
				return (int) MapPosition.GetScreenPosition().Y + Sprite.Height;
			}
		}

		public bool IsAtPoint(MapPoint checkPoint)
		{
			return MapPosition.IsAtPoint(checkPoint);
		}

		public abstract MapPoint MapPosition { get; set; }
		public abstract Texture2D Sprite { get; set; }

		public abstract void Draw(SpriteBatch spriteBatch);
	}
}
