using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class FadingTextPanel : UIComponent
	{
		private SpriteFont font;
		private Texture2D panelTexture;
		private Texture2D shadowTexture;

		private bool showing = false;
		private bool startShowing = false;
		private TimeSpan startedShowingAt;

		public FadingTextPanel(int x, int y)
		{
			X = x;
			Y = y;
		}

		public TimeSpan ShowFor { get; set; } = new TimeSpan(0, 0, 10);
		public string Text { get; set; }

		public void Show()
		{
			startShowing = true;
		}

		public void CreateGradient(int width, int height, GraphicsDevice graphicsDevice)
		{
			panelTexture = new Texture2D(graphicsDevice, width, height);
			var backgroundColour = new Color[height * width];
			var startingShade = 150;

			for (var i = 0; i < backgroundColour.Length; i++)
			{
				var textColour = startingShade + i / (height * 2);
				backgroundColour[i] = new Color(textColour, textColour, textColour, 0);
			}

			panelTexture.SetData(backgroundColour);
		}

		public override void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
		{
			shadowTexture = new Texture2D(graphicsDevice, 1, 1);
			shadowTexture.SetData(new Color[] { Color.Black });

			font = content.Load<SpriteFont>("fonts/Carlito-Regular");
		}

		public override void Update(GameTime gameTime)
		{
			if (startShowing)
			{
				startedShowingAt = gameTime.TotalGameTime;
				startShowing = false;
				showing = true;
			}

			if (showing && gameTime.TotalGameTime - startedShowingAt >= ShowFor)
			{
				showing = false;
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!showing)
				return;

			base.Draw(spriteBatch);

			var textSize = font.MeasureString(Text);
			var width = (int) textSize.X + MARGIN_X * 2;
			var height = (int) textSize.Y + MARGIN_Y * 2;

			CreateGradient(width, height, spriteBatch.GraphicsDevice);

			spriteBatch.Draw(shadowTexture, new Rectangle(X + BOX_SHADOW_X, Y + BOX_SHADOW_Y, width, height), Color.White);
			spriteBatch.Draw(panelTexture, new Rectangle(X, Y, width, height), Color.White);
			spriteBatch.DrawString(font, Text, new Vector2(X + MARGIN_X, Y + MARGIN_Y), Color.Black);
		}
	}
}
