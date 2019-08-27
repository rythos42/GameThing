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

		public const int MAX_MAP_Y = 29;
		public const int MIN_MAP_Y = 0;
		public const int MAX_MAP_X = 29;
		public const int MIN_MAP_X = 0;

		public int X { get; set; }
		public int Y { get; set; }

		public Vector2 GetScreenPosition()
		{
			// http://clintbellanger.net/articles/isometric_math/
			// map is [0, 0] top, [0, 29] right, [29, 29] bottom, [29, 0] left
			return new Vector2(
				(X - Y) * TileWidth_Half,
				(X + Y) * TileHeight_Half - (MapHelper.GetHeightAtMapPoint(X, Y) * 32)
			);
		}

		public static MapPoint GetFromScreenPosition(Vector2 screenPosition)
		{
			for (var layerNumber = MapHelper.MaxLayer; layerNumber >= 0; layerNumber--)
			{
				var layerOffset = layerNumber * 32;
				var x = (int) (screenPosition.X / TileWidth_Half + (screenPosition.Y + layerOffset) / TileHeight_Half) / 2;
				var y = (int) ((screenPosition.Y + layerOffset) / TileHeight_Half - screenPosition.X / TileWidth_Half) / 2;

				if (MapHelper.GetHeightAtMapPoint(x, y) == layerNumber)
					return new MapPoint { X = x, Y = y };
			}

			return null;
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

		public bool IsWithinMap
		{
			get
			{
				return X >= MIN_MAP_X
					&& X <= MAX_MAP_X
					&& Y >= MIN_MAP_Y
					&& Y <= MAX_MAP_Y;
			}
		}

		public bool IsWithinRectangle(Rectangle rect)
		{
			return X >= rect.X
				&& X < rect.X + rect.Width
				&& Y >= rect.Y
				&& Y < rect.Y + rect.Height;
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
