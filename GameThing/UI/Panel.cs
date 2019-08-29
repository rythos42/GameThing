using System.Collections.Generic;
using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Panel : UIComponent
	{
		private Texture2D panelTexture;
		private Texture2D shadowTexture;

		public bool PlaceFromRight { get; set; }
		public List<UIComponent> Components { get; set; } = new List<UIComponent>();

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

		public override Vector2 MeasureContent()
		{
			var width = 0;
			var height = 0;
			Components.ForEach(component =>
			{
				var size = component.MeasureContent();
				width = MathHelper.Max(width, (int) size.X);
				height += (int) size.Y + MARGIN;
			});

			return new Vector2(width, height - MARGIN); // we add one extra margin in the loop above, remove it
		}

		protected virtual Vector2 GetDrawingPosition(GraphicsDevice graphicsDevice, int x, int y)
		{
			if (PlaceFromRight)
			{
				var width = MeasureContent().X + MARGIN * 2;
				return new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth - width - MARGIN, y);
			}
			else
			{
				return new Vector2(x, y);
			}
		}

		public override void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			shadowTexture = new Texture2D(graphicsDevice, 1, 1);
			shadowTexture.SetData(new Color[] { Color.Black });

			Components.ForEach(component => component.LoadContent(content, graphicsDevice));
		}

		public override void Draw(SpriteBatch spriteBatch, int x, int y)
		{
			var contentSize = MeasureContent();
			var width = (int) contentSize.X + PADDING * 2;
			var height = (int) contentSize.Y + PADDING * 2;

			CreatePanelGradient(width, height, spriteBatch.GraphicsDevice);

			var drawingPosition = GetDrawingPosition(spriteBatch.GraphicsDevice, x, y);
			int drawX = (int) drawingPosition.X;
			int drawY = (int) drawingPosition.Y;

			spriteBatch.Draw(shadowTexture, new Rectangle(drawX + BOX_SHADOW_X, drawY + BOX_SHADOW_Y, width, height), Color.White);
			spriteBatch.Draw(panelTexture, new Rectangle(drawX, drawY, width, height), Color.White);

			drawX += MARGIN;
			drawY += MARGIN;
			Components.ForEach(component =>
			{
				component.Draw(spriteBatch, drawX, drawY);
				drawY += MARGIN + (int) component.MeasureContent().Y;
			});

			IsVisible = true;
		}
	}
}
