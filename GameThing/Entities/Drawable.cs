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

		public virtual void DrawWithEffect(SpriteBatch spriteBatch) { }
		public virtual void DrawWithoutEffect(SpriteBatch spriteBatch) { }
		public virtual void Update(GameTime gameTime) { }
	}
}
