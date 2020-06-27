using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities
{
	public interface IDrawable
	{
		void DrawWithEffect(SpriteBatch spriteBatch);
		void DrawWithoutEffect(SpriteBatch spritebatch);
		void Update(GameTime gameTime);
		bool IsAtPoint(MapPoint checkPoint);

		int DrawOrder { get; }
		MapPoint MapPosition { get; set; }
		int SpriteHeight { get; }
		int SpriteWidth { get; }
	}
}
