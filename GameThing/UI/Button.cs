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
		private int lastDrawnX;
		private int lastDrawnY;
		private int width;
		private int height;
		private const int MINIMUM_BUTTON_WIDTH = 300;

		public Button(string text)
		{
			this.text = text;
		}

		public bool IsAtPoint(Vector2 checkPoint)
		{
			return
				lastDrawnX < checkPoint.X
				&& lastDrawnX + width > checkPoint.X
				&& lastDrawnY < checkPoint.Y
				&& lastDrawnY + height > checkPoint.Y;
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
			width = UseMinimumButtonSize ? MathHelper.Max(minimumTextWidth, MINIMUM_BUTTON_WIDTH) : minimumTextWidth;
			height = (int) textSize.Y + (PADDING * 2);
		}

		public override void Draw(SpriteBatch spriteBatch, int x, int y)
		{
			lastDrawnX = x;
			lastDrawnY = y;

			spriteBatch.Draw(shadowTexture, new Rectangle(x + BOX_SHADOW_X, y + BOX_SHADOW_Y, width, height), Color.White);
			spriteBatch.Draw(IsHighlighted ? highlightTexture : buttonTexture, new Rectangle(x, y, width, height), Color.White);
			spriteBatch.DrawString(font, text, new Vector2(x + PADDING, y + PADDING), Color.Black);

			IsVisible = true;
		}

		public override Vector2 MeasureContent()
		{
			return new Vector2(width, height);
		}
	}
}
