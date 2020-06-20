using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities
{
	public abstract class Drawable : IDrawable
	{
		public int DrawOrder
		{
			get
			{
				return (int) MapPosition.GetScreenPosition().Y + SpriteHeight;
			}
		}

		public bool IsAtPoint(MapPoint checkPoint)
		{
			return MapPosition.IsAtPoint(checkPoint);
		}

		public abstract MapPoint MapPosition { get; set; }
		public abstract int SpriteHeight { get; }
		public abstract int SpriteWidth { get; }

		public abstract void Draw(SpriteBatch spriteBatch);
		public abstract void Update(GameTime gameTime);
	}
}
