using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Panel : UIContainer
	{
		public Panel()
		{
			AutoDrawChildren = true;
		}

		public HorizontalAlignment HorizontalAlignment { get; set; }
		public Texture2D Background { get; set; }

		private Vector2 MeasureContent()
		{
			var width = 0;
			var height = 0;
			Components.ForEach(component =>
			{
				width = MathHelper.Max(width, (int) component.Width);
				height += (int) component.Height + MARGIN;
			});

			return new Vector2(width, height - MARGIN); // we add one extra margin in the loop above, remove it
		}

		protected virtual Vector2 GetDrawingPosition(GraphicsDevice graphicsDevice, float x, float y)
		{
			if (HorizontalAlignment == HorizontalAlignment.Right)
			{
				var width = MeasureContent().X + x;
				return new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth - width - MARGIN, y);
			}
			else
			{
				return new Vector2(x, y);
			}
		}

		public override void Draw(SpriteBatch spriteBatch, float x, float y)
		{
			var contentSize = MeasureContent();
			var drawingPosition = GetDrawingPosition(spriteBatch.GraphicsDevice, x, y);

			Width = (int) contentSize.X + PADDING * 2 + (ExtendedPadding ? 64 : 0);
			Height = (int) contentSize.Y + PADDING * 2 + (ExtendedPadding ? 64 : 0);
			X = (int) drawingPosition.X;
			Y = (int) drawingPosition.Y;

			if (Background != null)
				spriteBatch.Draw(Background, new Rectangle((int) X, (int) Y, Width, Height), Color.White);

			base.Draw(spriteBatch, x, y);
		}
	}
}
