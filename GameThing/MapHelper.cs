using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing
{
	public static class MapHelper
	{
		public static void DrawRange(int range, MapPoint initialPosition, SpriteBatch spriteBatch, Texture2D texture, Color color)
		{
			for (var i = -1 * range; i < range + 1; i++)
			{
				var mapX = initialPosition.X + i;
				if (mapX < MapPoint.MIN_MAP_X || mapX > MapPoint.MAX_MAP_X)
					continue;

				var absX = Math.Abs(i);

				for (var j = -1 * range; j < range + 1; j++)
				{
					if (absX + Math.Abs(j) > range)
						continue;

					var mapY = initialPosition.Y + j;
					if (mapY < MapPoint.MIN_MAP_Y || mapY > MapPoint.MAX_MAP_Y)
						continue;

					var screenPosition = new MapPoint { X = mapX, Y = mapY }.GetScreenPosition();
					spriteBatch.Draw(texture, new Rectangle((int) screenPosition.X - MapPoint.TileWidth_Half, (int) screenPosition.Y, 64, 32), color * 0.5f);
				}
			}
		}
	}
}
