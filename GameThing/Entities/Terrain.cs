﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities
{
	public class Terrain : Drawable
	{
		public Terrain(Texture2D sprite, MapPoint mapPosition)
		{
			Sprite = sprite;
			MapPosition = mapPosition;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var drawPosition = MapPosition.GetScreenPosition();
			drawPosition.Y -= Sprite.Height - MapPoint.TileHeight;
			drawPosition.X -= Sprite.Width / 2;
			spriteBatch.Draw(Sprite, drawPosition, Color.White);
		}

		public override Texture2D Sprite { get; set; }
		public override MapPoint MapPosition { get; set; }
	}
}