using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Panel : UIContainer
	{
		[XmlAttribute]
		public SizingMode SizingMode { get; set; }

		[XmlAttribute]
		public LayoutDirection LayoutDirection { get; set; }

		[XmlAttribute]
		public bool AutoDrawChildren { get; set; } = true;

		[XmlAttribute]
		public LayoutMode LayoutMode { get; set; }

		public Vector2 MeasureContent()
		{
			var width = 0;
			var height = 0;
			Components.ForEach(component =>
			{
				if (LayoutDirection == LayoutDirection.Vertical)
				{
					width = MathHelper.Max(width, component.Width + (component.Margin * 2));
					height += component.Height + (component.Margin * 2);
				}
				else
				{
					width += component.Width + (component.Margin * 2);
					height = MathHelper.Max(height, component.Height + (component.Margin * 2));
				}
			});

			return new Vector2(width, height);
		}

		protected override void DrawComponent(SpriteBatch spriteBatch)
		{
			if (LayoutMode == LayoutMode.Flow && SizingMode == SizingMode.FitContent)
			{
				var contentSize = MeasureContent();
				Width = (int) contentSize.X + (Padding * 2);
				Height = (int) contentSize.Y + (Padding * 2);
			}

			base.DrawComponent(spriteBatch);

			if (!AutoDrawChildren)
				return;

			var drawX = X + Padding;
			var drawY = Y + Padding;

			Components.ForEach(component =>
			{
				if (LayoutDirection == LayoutDirection.Vertical)
					drawY += component.Margin;
				else
					drawX += component.Margin;

				if (LayoutMode == LayoutMode.Relative)
				{
					var x = X + component.X + component.Margin;
					var y = Y + component.Y + component.Margin;
					component.Draw(spriteBatch, x, y);
				}
				else
				{
					component.Position = new Vector2(drawX, drawY);
					component.Draw(spriteBatch);
				}

				if (LayoutMode == LayoutMode.Flow)
				{
					if (LayoutDirection == LayoutDirection.Vertical)
						drawY += (2 * component.Margin) + component.Height;
					else
						drawX += (2 * component.Margin) + component.Width;
				}
			});
		}
	}
}
