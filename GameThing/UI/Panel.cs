using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Panel : UIComponent
	{
		private Texture2D panelTexture;
		private Texture2D shadowTexture;

		public bool PlaceFromRight { get; set; }

		private void CreatePanelGradient(int width, int height, GraphicsDevice graphicsDevice)
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

		protected virtual Vector2 MeasureContent()
		{
			throw new NotImplementedException();
		}

		protected virtual Vector2 GetDrawingPosition(GraphicsDevice graphicsDevice)
		{
			if (PlaceFromRight)
			{
				var width = MeasureContent().X + MARGIN_X * 2;
				return new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth - width - MARGIN_X, Y);
			}
			else
			{
				return new Vector2(X, Y);
			}
		}

		public override void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
		{
			shadowTexture = new Texture2D(graphicsDevice, 1, 1);
			shadowTexture.SetData(new Color[] { Color.Black });
		}

		public override void Update(GameTime gameTime)
		{
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var contentSize = MeasureContent();
			var width = (int) contentSize.X + MARGIN_X * 2;
			var height = (int) contentSize.Y + MARGIN_Y * 2;

			CreatePanelGradient(width, height, spriteBatch.GraphicsDevice);

			var drawingPosition = GetDrawingPosition(spriteBatch.GraphicsDevice);
			int x = (int) drawingPosition.X;
			int y = (int) drawingPosition.Y;

			spriteBatch.Draw(shadowTexture, new Rectangle(x + BOX_SHADOW_X, y + BOX_SHADOW_Y, width, height), Color.White);
			spriteBatch.Draw(panelTexture, new Rectangle(x, y, width, height), Color.White);

			base.Draw(spriteBatch);
		}
	}
}
