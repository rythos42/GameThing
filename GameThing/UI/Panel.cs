using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Panel : UIContainer
	{
		private Texture2D panelTexture;

		public bool PlaceFromRight { get; set; }
		public bool ShowChrome { get; set; } = true;
		public bool ExtendedPadding { get; set; } = false;

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

		protected override void LoadComponentContent(Content content, GraphicsDevice graphicsDevice)
		{
			base.LoadComponentContent(content, graphicsDevice);

			panelTexture = content.PanelBackground;
		}

		public override void Draw(SpriteBatch spriteBatch, float x, float y)
		{
			var contentSize = MeasureContent();
			Width = (int) contentSize.X + PADDING * 2 + (ExtendedPadding ? 64 : 0);
			Height = (int) contentSize.Y + PADDING * 2 + (ExtendedPadding ? 64 : 0);

			var drawingPosition = GetDrawingPosition(spriteBatch.GraphicsDevice, x, y);
			X = (int) drawingPosition.X;
			Y = (int) drawingPosition.Y;

			if (ShowChrome)
				spriteBatch.Draw(panelTexture, new Rectangle((int) X, (int) Y, Width, Height), Color.White);

			var drawX = X + MARGIN + (ExtendedPadding ? 32 : 0);
			var drawY = Y + MARGIN + (ExtendedPadding ? 32 : 0);
			Components.ForEach(component =>
			{
				component.Draw(spriteBatch, drawX, drawY);
				drawY += MARGIN + (int) component.MeasureContent().Y;
			});

			IsVisible = true;
		}
	}
}
