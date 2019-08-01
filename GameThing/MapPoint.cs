﻿using System;
using Microsoft.Xna.Framework;

namespace GameThing
{
	public class MapPoint
	{
		private const int TileWidth = 64;
		private const int TileWidth_Half = TileWidth / 2;
		private const int TileHeight = 32;
		private const int TileHeight_Half = TileHeight / 2;

		public int X { get; set; }
		public int Y { get; set; }

		public Vector2 GetScreenPosition()
		{
			// http://clintbellanger.net/articles/isometric_math/
			// map is [0, 0] top, [0, 29] right, [29, 29] bottom, [29, 0] left
			return new Vector2(
				(X - Y) * TileWidth_Half,
				(X + Y) * TileHeight_Half
			);
		}

		public static MapPoint GetFromScreenPosition(Vector2 screenPosition)
		{
			return new MapPoint
			{
				X = (int) (screenPosition.X / TileWidth_Half + screenPosition.Y / TileHeight_Half) / 2,
				Y = (int) (screenPosition.Y / TileHeight_Half - screenPosition.X / TileWidth_Half) / 2
			};
		}

		public bool IsWithinDistanceOf(int range, MapPoint checkPoint)
		{
			var xDistance = Math.Abs(X - checkPoint.X);
			var yDistance = Math.Abs(Y - checkPoint.Y);
			return xDistance + yDistance <= range;
		}

		public bool IsAtPoint(MapPoint checkPoint)
		{
			return X == checkPoint.X && Y == checkPoint.Y;
		}
	}
}
