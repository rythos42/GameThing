using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities.Cards
{
	[DataContract]
	public abstract class Card
	{
		private Texture2D sprite;
		private Texture2D availableRangeTexture;
		private SpriteFont font;
		private Effect selectedCardEffect;
		private const int CARD_MARGIN = 20;

		public Card(string title, string description, int range, Character ownerCharacter)
		{
			Title = title;
			Description = description;
			Range = range;
			OwnerCharacter = ownerCharacter;
		}

		public void SetContent(Content content)
		{
			sprite = content.Card;
			availableRangeTexture = content.DistanceOverlay;
			font = content.Font;
			selectedCardEffect = content.Highlight;
		}

		public abstract void Play(int roundNumber, Character target = null);

		public bool IsWithinRangeDistance(MapPoint checkPoint)
		{
			return OwnerCharacter.IsWithinDistanceOf(Range, checkPoint);
		}

		public void DrawCard(SpriteBatch spriteBatch, Vector2 position)
		{
			spriteBatch.Draw(sprite, position, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
		}

		public void DrawCardText(SpriteBatch spriteBatch, Vector2 position)
		{
			var textPosition = new Vector2
			{
				X = position.X + CARD_MARGIN,
				Y = position.Y + CARD_MARGIN
			};
			spriteBatch.DrawString(font, Title, textPosition, Color.Black);

			textPosition.Y += font.LineSpacing;
			var maxLineWidth = Width - 2 * CARD_MARGIN;
			spriteBatch.DrawString(font, Description.WrapText(font, maxLineWidth), textPosition, Color.Black);
		}

		public void DrawSelectedCardBlurShader(SpriteBatch spriteBatch, Vector2 position)
		{
			selectedCardEffect.Parameters["BloomThreshold"].SetValue(0.25f);
			selectedCardEffect.Parameters["BloomIntensity"].SetValue(1.25f);
			selectedCardEffect.Parameters["BaseIntensity"].SetValue(1f);
			selectedCardEffect.Parameters["BloomSaturation"].SetValue(1f);
			selectedCardEffect.Parameters["BaseSaturation"].SetValue(1f);

			spriteBatch.Begin(
				sortMode: SpriteSortMode.BackToFront,
				blendState: BlendState.AlphaBlend,
				samplerState: SamplerState.AnisotropicWrap,
				effect: selectedCardEffect);

			DrawCard(spriteBatch, position);

			spriteBatch.End();
		}

		public void DrawEffectRange(SpriteBatch spriteBatch)
		{
			MapHelper.DrawRange(Range, OwnerCharacter.MapPosition, spriteBatch, availableRangeTexture, Color.Aqua);
		}

		public void Discard()
		{
			InHand = false;
			InDiscard = true;
		}

		public int Width { get { return sprite.Width / 2; } }
		public int Height { get { return sprite.Height / 2; } }
		public Character OwnerCharacter { get; set; }

		[DataMember]
		public bool InHand { get; set; }

		[DataMember]
		public bool InDiscard { get; set; }

		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public int Range { get; set; }
	}
}
