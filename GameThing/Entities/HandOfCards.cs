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
		private List<CardPosition> cardPositions = new List<CardPosition>();

		private class CardPosition
		{
			public RectangleF Position { get; set; }
			public Vector2 Origin { get; set; }
			public Card Card { get; set; }
		}

		public int GetCardIndexAtPosition(Vector2 position)
		{
			return cardPositions.FindLastIndex(cardPosition =>
				cardPosition.Position.Top < position.Y
				&& cardPosition.Position.Bottom > position.Y
				&& cardPosition.Position.Left < position.X
				&& cardPosition.Position.Right > position.X
			);
		}

		public void ClearCardPositions()
		{
			cardPositions.Clear();
		}

		public void Draw(SpriteBatch spriteBatch, Rectangle clientBounds, IEnumerable<Card> hand, Card selectedCard)
		{
			var totalWidth = hand.Aggregate(0, (total, card) => card.Width + total);
			var remainingWidth = clientBounds.Width - totalWidth;
			var spacing = remainingWidth / 5;
			var position = new Vector2
			{
				X = spacing,
				Y = clientBounds.Height / 2 + 250
			};
			CardPosition selectedCardPosition = null;

			cardPositions.Clear();
			foreach (var card in hand)
			{
				var cardPosition = new CardPosition
				{
					Origin = position,
					Position = new RectangleF
					{
						Height = card.Height,
						Width = card.Width,
						X = position.X,
						Y = position.Y
					},
					Card = card
				};

				cardPositions.Add(cardPosition);

				position.X += card.Width + spacing;

				if (card == selectedCard)
					selectedCardPosition = cardPosition;
			}

			if (selectedCardPosition != null)
				selectedCardPosition.Card.DrawSelectedCardBlurShader(spriteBatch, selectedCardPosition.Origin);

			spriteBatch.Begin();
			foreach (var cardPosition in cardPositions)
			{
				if (cardPosition == selectedCardPosition)
				{
					cardPosition.Card.DrawCardText(spriteBatch, cardPosition.Origin);
					continue;
				}

				cardPosition.Card.Draw(spriteBatch, cardPosition.Origin);
			}
			spriteBatch.End();
		}
	}
}
