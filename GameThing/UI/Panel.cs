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

		protected virtual Vector2 GetDrawingPosition(GraphicsDevice graphicsDevice, float x, float y)
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

			panelTexture = new Texture2D(graphicsDevice, 1, 1);
			panelTexture.SetData(new Color[] { Color.Linen });

			Components.ForEach(component => component.LoadContent(content, graphicsDevice));
		}

		public override void Draw(SpriteBatch spriteBatch, float x, float y)
		{
			var contentSize = MeasureContent();
			Width = (int) contentSize.X + PADDING * 2;
			Height = (int) contentSize.Y + PADDING * 2;

			var drawingPosition = GetDrawingPosition(spriteBatch.GraphicsDevice, x, y);
			X = (int) drawingPosition.X;
			Y = (int) drawingPosition.Y;

			spriteBatch.Draw(shadowTexture, new Rectangle((int) X + BOX_SHADOW_X, (int) Y + BOX_SHADOW_Y, Width, Height), Color.White);
			spriteBatch.Draw(panelTexture, new Rectangle((int) X, (int) Y, Width, Height), Color.White);

			var drawX = X + MARGIN;
			var drawY = Y + MARGIN;
			Components.ForEach(component =>
			{
				component.Draw(spriteBatch, drawX, drawY);
				drawY += MARGIN + (int) component.MeasureContent().Y;
			});

			IsVisible = true;
		}
	}
}
