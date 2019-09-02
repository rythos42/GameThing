using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GameThing.Entities.Cards;
using GameThing.Entities.Cards.Conditions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities
{
	[DataContract]
	public class Character : Drawable
	{
		private Texture2D availableMovementTexture;
		private Texture2D lockSprite;
		private Effect selectedCharacterEffect;
		private static Random rng = new Random();

		public Character(CharacterSide side, CharacterColour colour, CharacterClass characterClass)
		{
			CharacterClass = characterClass;
			Side = side;
			Colour = colour;
		}

		public void SetContent(Content content)
		{
			Sprite = content.GetSpriteFor(this);
			availableMovementTexture = content.DistanceOverlay;
			lockSprite = content.Lock;
			selectedCharacterEffect = content.Highlight;
		}

		[DataMember]
		public override MapPoint MapPosition { get; set; }

		[DataMember]
		public decimal BaseStrength { get; private set; } = 1;

		[DataMember]
		public decimal BaseAgility { get; private set; } = 1;

		[DataMember]
		public decimal BaseIntelligence { get; private set; } = 1;

		[DataMember]
		public decimal CurrentMaxHealth { get; private set; } = 5;

		[DataMember]
		public decimal StrengthMultiplier { get; set; } = 1;

		[DataMember]
		public decimal AgilityMultiplier { get; set; } = 1;

		[DataMember]
		public decimal IntelligenceMultiplier { get; set; } = 1;

		[DataMember]
		public decimal CurrentHealth { get; set; } = 5;

		[DataMember]
		public CharacterSide Side { get; set; }

		[DataMember]
		public CharacterClass CharacterClass { get; set; }

		[DataMember]
		public CharacterColour Colour { get; set; }

		[DataMember]
		public Character NextCardMustTarget { get; set; }

		public decimal CurrentStrength { get { return BaseStrength * StrengthMultiplier; } }
		public decimal CurrentAgility { get { return BaseAgility * AgilityMultiplier; } }
		public decimal CurrentIntelligence { get { return BaseIntelligence * IntelligenceMultiplier; } }

		public void ApplyDamage(decimal damageAmount)
		{
			CurrentHealth -= damageAmount;
		}

		public void ApplyHealing(decimal damageAmount)
		{
			CurrentHealth += damageAmount;
			if (CurrentHealth > CurrentMaxHealth)
				CurrentHealth = CurrentMaxHealth;
		}

		[DataMember]
		public List<AppliedCondition> Conditions { get; } = new List<AppliedCondition>();

		public void RemoveConditions(ConditionEndsOn endsOn, Predicate<AppliedCondition> appliesTo = null)
		{
			for (var i = Conditions.Count - 1; i >= 0; i--)
			{
				var condition = Conditions[i];

				if (condition.Condition.EndsOn == endsOn && (appliesTo?.Invoke(condition) ?? true) == true)
				{
					condition.Condition.Remove(this);
					Conditions.Remove(condition);
				}
			}
		}

		public void StartNewRound(int roundNumber)
		{
			RemoveConditions(ConditionEndsOn.StartRound, condition => condition.RoundNumber + condition.Condition.TurnCount == roundNumber);

			ResetTurn();
		}

		public void EndTurn()
		{
			ActivatedThisRound = true;
		}

		public void ResetTurn()
		{
			RemainingMoves = MaximumMoves;
			RemainingPlayableCards = MaximumPlayableCards;
			ActivatedThisRound = false;
		}

		[DataMember]
		public int MaximumMoves { get; set; } = 5;

		[DataMember]
		public int RemainingMoves { get; set; }

		[DataMember]
		public int RemainingPlayableCards { get; private set; }

		[DataMember]
		public int MaximumPlayableCards { get; private set; } = 2;

		[DataMember]
		public bool ActivatedThisRound { get; private set; }

		[DataMember]
		public List<Card> Deck { get; private set; } = new List<Card>();

		public bool HasRemainingPlayableCards { get { return RemainingPlayableCards > 0; } }
		public bool HasRemainingMoves { get { return RemainingMoves > 0; } }

		public void InitializeDeck()
		{
			Deck = new CardManager().CreateDefaultDeck(this);
			Shuffle(Deck);
			DrawOneCard();
			DrawOneCard();
			DrawOneCard();
			DrawOneCard();
		}

		public void PlayCard(Card card, Character targetCharacter, int roundNumber)
		{
			RemainingPlayableCards--;
			card.Play(roundNumber, targetCharacter);
			card.Discard();
			DrawOneCard();
		}

		private void Shuffle(List<Card> list)
		{
			var n = list.Count;
			while (n > 1)
			{
				n--;
				var k = rng.Next(n + 1);
				Card value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public void DrawOneCard()
		{
			var nextCard = Deck.FirstOrDefault(card => !card.InHand && !card.InDiscard);
			if (nextCard != null)
			{
				nextCard.InHand = true;
			}
			else
			{
				Deck.ForEach(card => card.InDiscard = false);

				var currentDeck = Deck.Where(card => !card.InHand).ToList();
				Shuffle(currentDeck);
				var currentHand = CurrentHand.ToList();

				Deck.Clear();
				Deck.AddRange(currentHand);
				Deck.AddRange(currentDeck);

				DrawOneCard();
			}
		}

		public IEnumerable<Card> CurrentHand
		{
			get
			{
				return Deck.Where(card => card.InHand);
			}
		}

		public int CardsInDeckCount
		{
			get
			{
				return Deck.Count(card => !card.InHand && !card.InDiscard);
			}
		}

		public int CardsInDiscardCount
		{
			get
			{
				return Deck.Count(card => card.InDiscard);
			}
		}

		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context)
		{
			// We don't serialize the OwnerCharacter for cards as it's wasteful, need to set after we build the Character and Deck
			Deck.ForEach(card => card.OwnerCharacter = this);
		}

		public bool IsWithinDistanceOf(int range, MapPoint checkPoint)
		{
			return MapPosition.IsWithinDistanceOf(range, checkPoint);
		}

		public bool IsWithinMoveDistanceOf(MapPoint checkPoint)
		{
			return IsWithinDistanceOf(RemainingMoves, checkPoint);
		}

		public void Move(MapPoint movePoint)
		{
			var distance = Math.Abs(MapPosition.X - movePoint.X) + Math.Abs(MapPosition.Y - movePoint.Y);

			RemainingMoves -= distance;
			MapPosition = movePoint;

			RemoveConditions(ConditionEndsOn.Move);
		}

		public override Texture2D Sprite { get; set; }

		public override void Draw(SpriteBatch spriteBatch)
		{
			var drawPosition = MapPosition.GetScreenPosition();
			drawPosition.Y -= Sprite.Height / 1.3f;
			drawPosition.X -= Sprite.Width / 2;

			spriteBatch.Draw(Sprite, drawPosition, Color.White);
		}

		public void DrawSelectedCharacter(SpriteBatch spriteBatch, Matrix transformMatrix)
		{
			selectedCharacterEffect.Parameters["BloomThreshold"].SetValue(0.25f);
			selectedCharacterEffect.Parameters["BloomIntensity"].SetValue(1.25f);
			selectedCharacterEffect.Parameters["BaseIntensity"].SetValue(1f);
			selectedCharacterEffect.Parameters["BloomSaturation"].SetValue(1f);
			selectedCharacterEffect.Parameters["BaseSaturation"].SetValue(1f);

			spriteBatch.Begin(
				transformMatrix: transformMatrix,
				sortMode: SpriteSortMode.BackToFront,
				blendState: BlendState.AlphaBlend,
				samplerState: SamplerState.AnisotropicWrap,
				effect: selectedCharacterEffect);

			Draw(spriteBatch);

			spriteBatch.End();
		}

		public void DrawLock(SpriteBatch spriteBatch)
		{
			var drawPosition = MapPosition.GetScreenPosition();
			drawPosition.Y -= Sprite.Height - lockSprite.Height / 2;
			drawPosition.X -= lockSprite.Width / 2;

			spriteBatch.Draw(lockSprite, drawPosition, Color.White);
		}

		public void DrawMovementRange(SpriteBatch spriteBatch)
		{
			MapHelper.DrawRange(RemainingMoves, MapPosition, spriteBatch, availableMovementTexture, Color.White, showUnderCharacters: false);
		}
	}
}
