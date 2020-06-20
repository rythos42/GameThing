using System;
using System.Linq;
using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;

namespace GameThing
{
	public static class MapHelper
	{
		private static TiledMap map;
		private static int[] layerNumberList;

		public static TiledMap Map
		{
			get { return map; }
			set
			{
				map = value;
				layerNumberList = new int[map.Height * map.Width];

				for (ushort i = 0; i < map.Width; i++)
				{
					for (ushort j = 0; j < map.Height; j++)
					{
						for (var layerIndex = 0; layerIndex < map.TileLayers.Count; layerIndex++)
						{
							var tileLayer = map.TileLayers[layerIndex];
							TiledMapTile? outTile;
							if (tileLayer.TryGetTile(i, j, out outTile) && !outTile.Value.IsBlank)
							{
								var layerNumber = ((int) tileLayer.Offset.Y / -32) - 1;
								layerNumberList[i + j * map.Width] = layerNumber;
								MaxLayer = MathHelper.Max(MaxLayer, layerNumber);
							}
						}
					}
				}
			}
		}

		public static int MapHeight { get { return map.Height; } }
		public static int MapWidth { get { return map.Width; } }
		public static int MaxLayer { get; private set; }
		public static DrawableList Entities { get; set; }

		public static int GetHeightAtMapPoint(int x, int y)
		{
			var index = x + y * map.Width;
			if (index >= layerNumberList.Length)
				return -1;

			return layerNumberList[index];
		}

		public static Rectangle GetObjectRectangleInMapPoints(TiledMapObject deployment)
		{
			return new Rectangle(
				(int) Math.Round(deployment.Position.X / 32, 0, MidpointRounding.AwayFromZero),
				(int) Math.Round(deployment.Position.Y / 32, 0, MidpointRounding.AwayFromZero),
				(int) Math.Round(deployment.Size.Width / 32, 0, MidpointRounding.AwayFromZero),
				(int) Math.Round(deployment.Size.Height / 32, 0, MidpointRounding.AwayFromZero));
		}

		public static bool IsInAvailableMovement(MapPoint mapPosition)
		{
			var noAvailableMovementGroundLayer = Map.GetLayer<TiledMapObjectLayer>($"NoAvailableMovement:{mapPosition.Height}");
			return noAvailableMovementGroundLayer == null || !noAvailableMovementGroundLayer.Objects.Any(mapObject => mapPosition.IsWithinRectangle(GetObjectRectangleInMapPoints(mapObject)));
		}

		public static void DrawRange(int range, MapPoint initialPosition, SpriteBatch spriteBatch, Texture2D texture, Color color, bool showUnderCharacters)
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

					var mapPosition = new MapPoint(mapX, mapY);
					if (!mapPosition.IsInAvailableMovement)
						continue;

					if ((showUnderCharacters && !Entities.IsTerrainAtPoint(mapPosition)) || !Entities.IsEntityAtPoint(mapPosition))
					{
						var screenPosition = mapPosition.GetScreenPosition();
						spriteBatch.Draw(texture, new Rectangle((int) screenPosition.X - MapPoint.TileWidth_Half, (int) screenPosition.Y, 64, 32), color * 0.5f);
					}
				}
			}
		}
	}
}
