using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing
{
	public static class StringHelper
	{
		public static string WrapText(this string text, SpriteFont font, float maxLineWidth)
		{
			var words = text.Split(' ');
			var sb = new StringBuilder();
			var lineWidth = 0f;
			var spaceWidth = font.MeasureString(" ").X;

			foreach (string word in words)
			{
				Vector2 size = font.MeasureString(word);

				if (lineWidth + size.X < maxLineWidth)
				{
					sb.Append(word + " ");
					lineWidth += size.X + spaceWidth;
				}
				else
				{
					if (size.X > maxLineWidth)
					{
						if (sb.ToString() == "")
						{
							sb.Append(WrapText(word.Insert(word.Length / 2, " ") + " ", font, maxLineWidth));
						}
						else
						{
							sb.Append("\n" + WrapText(word.Insert(word.Length / 2, " ") + " ", font, maxLineWidth));
						}
					}
					else
					{
						sb.Append("\n" + word + " ");
						lineWidth = size.X + spaceWidth;
					}
				}
			}

			return sb.ToString();
		}
	}
}
