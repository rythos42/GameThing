using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities.Cards
{
	[DataContract]
	public abstract class Card
	{
		private Texture2D sprite;
		private SpriteFont font;
		private Texture2D availableRangeTexture;
		private Effect selectedCardBlur;
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
			sprite = content.CardSprite;
			font = content.Font;
			availableRangeTexture = content.DistanceOverlay;
			selectedCardBlur = content.GaussianBlur;
		}

		public abstract void Play(int roundNumber, Character target = null);

		public bool IsWithinRangeDistance(MapPoint checkPoint)
		{
			return OwnerCharacter.IsWithinDistanceOf(Range, checkPoint);
		}

		public void Draw(SpriteBatch spriteBatch, Vector2 position)
		{
			spriteBatch.Draw(sprite, position, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
			DrawCardText(spriteBatch, position);
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
			SetBlurEffectParameters(1.0f / Width, 0);
			spriteBatch.Begin(
				sortMode: SpriteSortMode.BackToFront,
				blendState: BlendState.AlphaBlend,
				samplerState: SamplerState.AnisotropicWrap,
				effect: selectedCardBlur);

			Draw(spriteBatch, position);

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

		private void SetBlurEffectParameters(float dx, float dy)
		{
			// Look up the sample weight and offset effect parameters.
			var weightsParameter = selectedCardBlur.Parameters["SampleWeights"];
			var offsetsParameter = selectedCardBlur.Parameters["SampleOffsets"];

			// Look up how many samples our Gaussian blur effect supports.
			var sampleCount = weightsParameter.Elements.Count;

			// Create temporary arrays for computing our filter settings.
			var sampleWeights = new float[sampleCount];
			var sampleOffsets = new Vector2[sampleCount];

			// The first sample always has a zero offset.
			sampleWeights[0] = ComputeGaussian(0);
			sampleOffsets[0] = new Vector2(0);

			// Maintain a sum of all the weighting values.
			var totalWeights = sampleWeights[0];

			// Add pairs of additional sample taps, positioned
			// along a line in both directions from the center.
			for (var i = 0; i < sampleCount / 2; i++)
			{
				// Store weights for the positive and negative taps.
				var weight = ComputeGaussian(i + 1);

				sampleWeights[i * 2 + 1] = weight;
				sampleWeights[i * 2 + 2] = weight;

				totalWeights += weight * 2;

				// To get the maximum amount of blurring from a limited number of
				// pixel shader samples, we take advantage of the bilinear filtering
				// hardware inside the texture fetch unit. If we position our texture
				// coordinates exactly halfway between two texels, the filtering unit
				// will average them for us, giving two samples for the price of one.
				// This allows us to step in units of two texels per sample, rather
				// than just one at a time. The 1.5 offset kicks things off by
				// positioning us nicely in between two texels.
				var sampleOffset = i * 2 + 1.5f;

				var delta = new Vector2(dx, dy) * sampleOffset;

				// Store texture coordinate offsets for the positive and negative taps.
				sampleOffsets[i * 2 + 1] = delta;
				sampleOffsets[i * 2 + 2] = -delta;
			}

			// Normalize the list of sample weightings, so they will always sum to one.
			for (var i = 0; i < sampleWeights.Length; i++)
				sampleWeights[i] /= totalWeights;

			// Tell the effect about our new filter settings.
			weightsParameter.SetValue(sampleWeights);
			offsetsParameter.SetValue(sampleOffsets);
		}

		/// <summary>
		///     Evaluates a single point on the Gaussian falloff curve. Used for setting up the blur filter weightings.
		/// </summary>
		private static float ComputeGaussian(float n)
		{
			var theta = 20;
			return (float) (1.0 / Math.Sqrt(2 * Math.PI * theta) * Math.Exp(-(n * n) / (2 * theta * theta)));
		}
	}
}
