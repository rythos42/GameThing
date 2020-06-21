using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GameThing.Contract;
using GameThing.Database;
using GameThing.Entities.Cards.Conditions;
using GameThing.Entities.Cards.Requirements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Scriban;
using Scriban.Runtime;

namespace GameThing.Entities.Cards
{
	[DataContract]
	public class Card : IIdentifiable
	{
		private Texture2D sprite;
		private Texture2D availableRangeTexture;
		private SpriteFont font;
		private const int cardMargin = 20;

		private Character ownerCharacter;
		private string description;

		private readonly ScriptObject templateScriptObject;
		private readonly TemplateContext textParsingTemplateContext;
		private Template descriptionTemplate;

		public Card()
		{
			templateScriptObject = new ScriptObject();
			templateScriptObject["card"] = this;

			// Any properties that start with "Templating", remove that. It's a marker that this property is intended only for use with the template engine.
			textParsingTemplateContext = new TemplateContext { MemberRenamer = member => member.Name.Replace("Templating", "") };
			textParsingTemplateContext.PushGlobal(templateScriptObject);
		}

		public void SetContent(Content content)
		{
			sprite = content.Card;
			availableRangeTexture = content.DistanceOverlay;
			font = content.Font;
		}

		public PlayStatus Play(Character target, int roundNumber, int turnNumber)
		{
			var applyValue = CardType == CardType.Damage || CardType == CardType.Heal
				? OwnerCharacter.ChangeDamageOrHealingForStamina(OwnerCharacter.GetCurrentAbilityScore(AbilityScore.Value) * EffectPercent.Value, turnNumber)
				: 0;

			var failedRequirements = Requirements.Where(requirement => !requirement.Met(OwnerCharacter, target));
			if (failedRequirements.Any())
				return new PlayStatus(PlayStatusDetails.FailedRequirement) { PlayCancelled = true, RequirementType = failedRequirements.First().Type };

			if (CardType == CardType.Damage)
			{
				target.Conditions.ForEach(condition => condition.Condition.ApplyEffects(OwnerCharacter, target));
				var playStatus = target.AttemptToApplyDamage(applyValue);
				target.RemoveConditions(ConditionEndsOn.AfterAttack);

				return playStatus;
			}
			else if (CardType == CardType.Heal)
			{
				return target.ApplyHealing(applyValue);
			}
			else if (CardType == CardType.Condition)
			{
				// if a non-stacking condition is on the target already, cancel the card play
				if (target.Conditions.Any(applied => applied.Condition.StackGroup == Condition.StackGroup))
					return new PlayStatus(PlayStatusDetails.FailedNoStack) { PlayCancelled = true };

				target.Conditions.Add(new AppliedCondition(Condition, roundNumber));
				Condition.ApplyEffects(OwnerCharacter, target);
			}

			return new PlayStatus(PlayStatusDetails.Success);
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
			var clonedCard = Contract.Convert.Clone(this);
			clonedCard.OwnerCharacter = character;
			if (clonedCard.Condition != null)
				clonedCard.Condition.OwningCharacter = character;
			return clonedCard;
		}

		public int Width => sprite.Width / 2;
		public int Height => sprite.Height / 2;
		public Character OwnerCharacter
		{
			get { return ownerCharacter; }
			set
			{
				ownerCharacter = value;
				templateScriptObject["character"] = value;
			}
		}

		[DataMember]
		public CardType CardType { get; set; }

		[DataMember]
		public bool InHand { get; set; }

		[DataMember]
		public bool InDiscard { get; set; }

		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public string Title { get; set; }

		public string FullDescription
		{
			get
			{
				return descriptionTemplate.Render(textParsingTemplateContext) + (Condition == null ? "" : (": " + Condition.Text));
			}
		}

		[DataMember]
		public string Description
		{
			get { return description; }
			set
			{
				description = value;
				descriptionTemplate = Template.Parse(value);
			}
		}

		[DataMember]
		public int Range { get; set; }

		[DataMember]
		public Condition Condition { get; set; }

		[DataMember]
		public decimal? EffectPercent { get; set; }

		[DataMember]
		public AbilityScore? AbilityScore { get; set; }

		[DataMember]
		[JsonConverter(typeof(IdentifierBasedConverter<Category>), typeof(CategoryMapper))]
		public List<Category> Categories { get; set; } = new List<Category>();

		[DataMember]
		public List<Requirement> Requirements { get; set; } = new List<Requirement>();
	}
}
