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

		public SizingMode SizingMode { get; set; }
		public Texture2D Background { get; set; }

		public Vector2 MeasureContent()
		{
			var width = 0;
			var height = 0;
			Components.ForEach(component =>
			{
				width = MathHelper.Max(width, component.Width);
				height += component.Height + MARGIN;
			});

			return new Vector2(width, height - MARGIN); // we add one extra margin in the loop above, remove it
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (SizingMode == SizingMode.FitContent)
			{
				var contentSize = MeasureContent();
				Width = (int) contentSize.X + (PADDING * 2) + (ExtendedPadding ? 64 : 0);
				Height = (int) contentSize.Y + (PADDING * 2) + (ExtendedPadding ? 64 : 0);
			}

			if (Background != null)
				spriteBatch.Draw(Background, new Rectangle((int) X, (int) Y, Width, Height), Color.White);

			base.Draw(spriteBatch);
		}
	}
}
