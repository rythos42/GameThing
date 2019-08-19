using System;
using Microsoft.Xna.Framework;

namespace GameThing
{
	public class MapPoint
	{
		public const int TileWidth = 64;
		public const int TileWidth_Half = TileWidth / 2;
		public const int TileHeight = 32;
		public const int TileHeight_Half = TileHeight / 2;

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

		public override bool Equals(object obj)
		{
			var otherPoint = obj as MapPoint;
			if (obj == null) return false;

			return otherPoint.X == X && otherPoint.Y == Y;
		}

		public override int GetHashCode()
		{
			var hashCode = 1861411795;
			hashCode = hashCode * -1521134295 + X.GetHashCode();
			hashCode = hashCode * -1521134295 + Y.GetHashCode();
			return hashCode;
		}
	}
}
