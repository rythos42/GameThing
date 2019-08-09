using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities.Content
{
	public class CardContent
	{
		private readonly CommonContent common;

		public CardContent(ContentManager content, CommonContent common)
		{
			Sprite = content.Load<Texture2D>("sprites/card");
			Font = content.Load<SpriteFont>("fonts/Carlito-Regular");

			this.common = common;
		}

		public Texture2D Sprite { get; private set; }
		public SpriteFont Font { get; private set; }
		public Texture2D AvailableRangeTexture { get { return common.DistanceOverlay; } }
	}
}
