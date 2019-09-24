using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Text : UIComponent
	{
		private SpriteFont font;

		public string Value { get; set; }

		public override Vector2 MeasureContent()
		{
			return font.MeasureString(Value);
		}

		public override void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			font = content.Font;
		}

		public override void Draw(SpriteBatch spriteBatch, int x, int y)
		{
			X = x;
			Y = y;

			spriteBatch.DrawString(font, Value, new Vector2(x, y), Color.Black);
			IsVisible = true;
		}
	}
}
