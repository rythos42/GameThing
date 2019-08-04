using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Button : UIComponent
	{
		private Texture2D buttonTexture;
		private Texture2D shadowTexture;
		private SpriteFont font;
		private readonly int height;
		private readonly int width;
		private readonly string text;

		public Button(string text, int x, int y, int width, int height)
		{
			this.text = text;
			this.width = width;
			this.height = height;
			X = x;
			Y = y;
		}

		public bool IsAtPoint(Vector2 checkPoint)
		{
			return
				X < checkPoint.X
				&& X + width > checkPoint.X
				&& Y < checkPoint.Y
				&& Y + height > checkPoint.Y;
		}

		public override void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
		{
			buttonTexture = new Texture2D(graphicsDevice, 1, 1);
			buttonTexture.SetData(new Color[] { Color.Silver });

			shadowTexture = new Texture2D(graphicsDevice, 1, 1);
			shadowTexture.SetData(new Color[] { Color.Black });

			font = content.Load<SpriteFont>("fonts/Carlito-Regular");
		}

		public override void Update(GameTime gameTime)
		{
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			var textSize = font.MeasureString(text);

			var horizontalMargin = (width - textSize.X) / 2;
			var verticalMargin = (height - textSize.Y) / 2;

			spriteBatch.Draw(shadowTexture, new Rectangle(X + BOX_SHADOW_X, Y + BOX_SHADOW_Y, width, height), Color.White);
			spriteBatch.Draw(buttonTexture, new Rectangle(X, Y, width, height), Color.White);
			spriteBatch.DrawString(font, text, new Vector2(X + horizontalMargin, Y + verticalMargin), Color.Black);
		}
	}
}
