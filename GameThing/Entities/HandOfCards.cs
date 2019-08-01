using System.Collections.Generic;
using System.Linq;
using GameThing.Entities.Cards;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace GameThing.Entities
{
	public class HandOfCards
	{
		private List<RectangleF> cardPositions = new List<RectangleF>();

		public int GetCardIndexAtPosition(Vector2 position)
		{
			return cardPositions.FindLastIndex(cardPosition =>
				cardPosition.Top < position.Y
				&& cardPosition.Bottom > position.Y
				&& cardPosition.Left < position.X
				&& cardPosition.Right > position.X
			);
		}

		public void ClearCardPositions()
		{
			cardPositions.Clear();
		}

		public void Draw(SpriteBatch spriteBatch, Rectangle clientBounds, IEnumerable<Card> hand)
		{
			cardPositions.Clear();

			var totalWidth = hand.Aggregate(0, (total, card) => card.Width + total);
			var remainingWidth = clientBounds.Width - totalWidth;
			var spacing = remainingWidth / 5;

			var position = new Vector2
			{
				X = spacing,
				Y = clientBounds.Height / 2 + 250
			};

			foreach (var card in hand)
			{
				card.Draw(spriteBatch, position);
				cardPositions.Add(new RectangleF
				{
					Height = card.Height,
					Width = card.Width,
					X = position.X,
					Y = position.Y
				});

				position.X += card.Width + spacing;
			}
		}
	}
}
