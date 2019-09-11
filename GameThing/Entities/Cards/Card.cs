using System.Collections.Generic;
using System.Runtime.Serialization;
using GameThing.Entities.Cards.Conditions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities.Cards
{
	[DataContract]
	public class Card
	{
		private Texture2D sprite;
		private Texture2D availableRangeTexture;
		private SpriteFont font;
		private Effect selectedCardEffect;
		private const int cardMargin = 20;

		public void SetContent(Content content)
		{
			sprite = content.Card;
			availableRangeTexture = content.DistanceOverlay;
			font = content.Font;
			selectedCardEffect = content.Highlight;
		}

		public void Play(int roundNumber, Character target = null)
		{
			if (CardType == Type.Damage)
			{
				decimal damage = 0;

				target.Conditions.ForEach(condition => condition.Condition.ApplyBeforeDamage(OwnerCharacter, target));

				switch (AbilityScore)
				{
					case AbilityScore.Agility: damage = OwnerCharacter.CurrentAgility * EffectPercent; break;
					case AbilityScore.Strength: damage = OwnerCharacter.CurrentStrength * EffectPercent; break;
					case AbilityScore.Intelligence: damage = OwnerCharacter.CurrentIntelligence * EffectPercent; break;
				}
				target.ApplyDamage(damage);

				target.RemoveConditions(ConditionEndsOn.AfterAttack);
			}
			else if (CardType == Type.Heal)
			{
				decimal healing = 0;
				switch (AbilityScore)
				{
					case AbilityScore.Agility: healing = OwnerCharacter.CurrentAgility * EffectPercent; break;
					case AbilityScore.Strength: healing = OwnerCharacter.CurrentStrength * EffectPercent; break;
					case AbilityScore.Intelligence: healing = OwnerCharacter.CurrentIntelligence * EffectPercent; break;
				}
				target.ApplyHealing(healing);
			}
			else if (CardType == Type.Condition)
			{
				target.Conditions.Add(new AppliedCondition(Condition, roundNumber));
				Condition.ApplyImmediately(target);
			}
		}

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
				X = position.X + cardMargin,
				Y = position.Y + cardMargin
			};
			spriteBatch.DrawString(font, Title, textPosition, Color.Black);

			textPosition.Y += font.LineSpacing;
			var maxLineWidth = Width - (2 * cardMargin);
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
			MapHelper.DrawRange(Range, OwnerCharacter.MapPosition, spriteBatch, availableRangeTexture, Color.Aqua, showUnderCharacters: true);
		}

		public void Discard()
		{
			InHand = false;
			InDiscard = true;
		}

		public int Width => sprite.Width / 2;
		public int Height => sprite.Height / 2;
		public Character OwnerCharacter { get; set; }

		[DataMember]
		public Type CardType { get; set; }

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

		[DataMember]
		public Condition Condition { get; set; }

		[DataMember]
		public decimal EffectPercent { get; set; }

		[DataMember]
		public AbilityScore AbilityScore { get; set; }

		[DataMember]
		public List<Category> Categories { get; set; } = new List<Category>();
	}
}
