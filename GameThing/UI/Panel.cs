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

		public Vector2 MeasureContent()
		{
			var width = 0;
			var height = 0;
			Components.ForEach(component =>
			{
				width = MathHelper.Max(width, component.Width + (component.Margin * 2));
				height += component.Height + (component.Margin * 2);
			});

			return new Vector2(width, height);
		}

		protected override void DrawComponent(SpriteBatch spriteBatch)
		{
			if (SizingMode == SizingMode.FitContent)
			{
				var contentSize = MeasureContent();
				Width = (int) contentSize.X + (Padding * 2);
				Height = (int) contentSize.Y + (Padding * 2);
			}

			base.DrawComponent(spriteBatch);
		}
	}
}
