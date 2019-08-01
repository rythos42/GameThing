using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Button
	{
		private Texture2D buttonTexture;
		private Texture2D shadowTexture;
		private SpriteFont font;
		private readonly int x;
		private readonly int y;
		private readonly int height;
		private readonly int width;
		private readonly string text;

		private const int BOX_SHADOW_X = 5;
		private const int BOX_SHADOW_Y = 5;

		public Button(string text, int x, int y, int width, int height)
		{
			this.text = text;
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}

		public bool IsVisible { get; set; }

		public bool IsAtPoint(Vector2 checkPoint)
		{
			return
				x < checkPoint.X
				&& x + width > checkPoint.X
				&& y < checkPoint.Y
				&& y + height > checkPoint.Y;
		}

		public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
		{
			buttonTexture = new Texture2D(graphicsDevice, 1, 1);
			buttonTexture.SetData(new Color[] { Color.Silver });

			shadowTexture = new Texture2D(graphicsDevice, 1, 1);
			shadowTexture.SetData(new Color[] { Color.Black });

			font = content.Load<SpriteFont>("fonts/Carlito-Regular");
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			var textSize = font.MeasureString(text);

			var horizontalMargin = (width - textSize.X) / 2;
			var verticalMargin = (height - textSize.Y) / 2;

			spriteBatch.Draw(shadowTexture, new Rectangle(x + BOX_SHADOW_X, y + BOX_SHADOW_Y, width, height), Color.White);
			spriteBatch.Draw(buttonTexture, new Rectangle(x, y, width, height), Color.White);
			spriteBatch.DrawString(font, text, new Vector2(x + horizontalMargin, y + verticalMargin), Color.Black);

			IsVisible = true;
		}
	}
}
