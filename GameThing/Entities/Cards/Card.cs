using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Antlr4.StringTemplate;
using GameThing.Data;
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
		private const int cardMargin = 20;

		public Card(int id)
		{
			Id = id;
		}

		public void SetContent(Content content)
		{
			sprite = content.Card;
			availableRangeTexture = content.DistanceOverlay;
			font = content.Font;
		}

		public bool Play(int roundNumber, Character target = null)
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
				// if the condition is on the target already, cancel the card play
				if (target.Conditions.Any(applied => applied.Condition.Id == Condition.Id))
					return false;

				target.Conditions.Add(new AppliedCondition(Condition, roundNumber));
				Condition.ApplyImmediately(target);
			}

			return true;
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
			spriteBatch.DrawString(font, FullDescription.WrapText(font, maxLineWidth), textPosition, Color.Black);
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

		public Card CreateForCharacter(Character character)
		{
			var cardBytes = Convert.Serialize(this);
			var clonedCard = Convert.Deserialize<Card>(cardBytes);
			clonedCard.OwnerCharacter = character;
			if (clonedCard.Condition != null)
				clonedCard.Condition.SourceCharacter = character;
			return clonedCard;
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
		public int Id { get; set; }

		[DataMember]
		public string Title { get; set; }

		public string FullDescription
		{
			get
			{
				var templateStr = Condition == null ? Description : Description + ": " + Condition.Text;
				var template = new Template(templateStr);
				template.Add("character", OwnerCharacter);
				template.Add("card", this);
				return template.Render();
			}
		}

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
