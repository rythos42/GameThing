using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Text : UIComponent
	{
		private SpriteFont font;
		private string value;

		public string Value
		{
			get { return value; }
			set
			{
				this.value = value;
				SetDimensions();
			}
		}

		private void SetDimensions()
		{
			Dimensions = Value == null || font == null ? Vector2.Zero : font.MeasureString(Value);
		}

		protected override void LoadComponentContent(Content content, GraphicsDevice graphicsDevice)
		{
			font = content.Font;
			SetDimensions();
		}

		public override void Draw(SpriteBatch spriteBatch, float x, float y)
		{
			X = x;
			Y = y;

			if (Value == null)
				return;

			spriteBatch.DrawString(font, Value, new Vector2(x, y), Color.Black);
			IsVisible = true;
		}
	}
}
