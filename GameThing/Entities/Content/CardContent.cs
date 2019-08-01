using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities.Content
{
	public class CardContent
	{
		public CardContent(ContentManager content)
		{
			Sprite = content.Load<Texture2D>("sprites/card");
			Font = content.Load<SpriteFont>("fonts/Carlito-Regular");
		}

		public Texture2D Sprite { get; set; }
		public SpriteFont Font { get; set; }
	}
}
