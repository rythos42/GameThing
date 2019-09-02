using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities
{
	public interface IDrawable
	{
		void Draw(SpriteBatch spriteBatch);
		bool IsAtPoint(MapPoint checkPoint);

		int DrawOrder { get; }
		Texture2D Sprite { get; set; }
		MapPoint MapPosition { get; set; }
	}
}
