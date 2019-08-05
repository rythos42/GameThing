using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class FadingTextPanel : Panel
	{
		private SpriteFont font;

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

		public void Show(string text)
		{
			Text = text;
			startShowing = true;
		}

		protected override Vector2 MeasureContent()
		{
			return font.MeasureString(Text);
		}

		public override void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
		{
			base.LoadContent(content, graphicsDevice);

			font = content.Load<SpriteFont>("fonts/Carlito-Regular");
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

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

			var drawingPosition = GetDrawingPosition(spriteBatch.GraphicsDevice);
			var x = (int) drawingPosition.X;
			var y = (int) drawingPosition.Y;

			spriteBatch.DrawString(font, Text, new Vector2(x + MARGIN_X, y + MARGIN_Y), Color.Black);
		}
	}
}
