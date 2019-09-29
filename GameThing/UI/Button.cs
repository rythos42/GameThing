using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Button : UIComponent
	{
		private Texture2D buttonTexture;
		private Texture2D highlightTexture;
		private Texture2D shadowTexture;
		private SpriteFont font;
		private readonly string text;
		private const int MINIMUM_BUTTON_WIDTH = 300;

		public Button(string text)
		{
			this.text = text;
		}

		public bool IsHighlighted { get; set; }
		public bool UseMinimumButtonSize { get; set; } = true;

		public override void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			buttonTexture = new Texture2D(graphicsDevice, 1, 1);
			buttonTexture.SetData(new Color[] { Color.Silver });

			highlightTexture = new Texture2D(graphicsDevice, 1, 1);
			highlightTexture.SetData(new Color[] { Color.OrangeRed });

			shadowTexture = new Texture2D(graphicsDevice, 1, 1);
			shadowTexture.SetData(new Color[] { Color.Black });

			font = content.Font;
			var textSize = font.MeasureString(text);
			var minimumTextWidth = (int) textSize.X + (PADDING * 2);
			Width = UseMinimumButtonSize ? MathHelper.Max(minimumTextWidth, MINIMUM_BUTTON_WIDTH) : minimumTextWidth;
			Height = (int) textSize.Y + (PADDING * 2);
		}

		public override void Draw(SpriteBatch spriteBatch, float x, float y)
		{
			X = x;
			Y = y;

			spriteBatch.Draw(shadowTexture, new Rectangle((int) x + BOX_SHADOW_X, (int) y + BOX_SHADOW_Y, Width, Height), Color.White);
			spriteBatch.Draw(IsHighlighted ? highlightTexture : buttonTexture, new Rectangle((int) x, (int) y, Width, Height), Color.White);
			spriteBatch.DrawString(font, text, new Vector2(x + PADDING, y + PADDING), Color.Black);

			IsVisible = true;
		}

		public override Vector2 MeasureContent()
		{
			return new Vector2(Width, Height);
		}
	}
}
