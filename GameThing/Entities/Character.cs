using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GameThing.Contract;
using GameThing.Database;
using GameThing.Entities.Cards;
using GameThing.Entities.Cards.Conditions;
using GameThing.Manager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace GameThing.Entities
{
	[DataContract]
	public class Character : Drawable
	{
		private Texture2D availableMovementTexture;
		private Texture2D lockSprite;
		private static readonly Random rng = new Random();

		public Character(Guid id, CharacterColour colour, CharacterClass characterClass)
		{
			Id = id;
			CharacterClass = characterClass;
			Colour = colour;
		}

		public void SetContent(Content content, CharacterSide side)
		{
			Sprite = content.GetSpriteFor(this, side);
			availableMovementTexture = content.DistanceOverlay;
			lockSprite = content.Lock;
		}

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public override MapPoint MapPosition { get; set; }

		[DataMember]
		public decimal BaseStrength { get; private set; } = 1;

		[DataMember]
		public decimal BaseAgility { get; private set; } = 1;

		[DataMember]
		public decimal BaseIntelligence { get; private set; } = 1;

		[DataMember]
		public decimal CurrentHealth { get; set; } = 7;

		[DataMember]
		public decimal CurrentMaxHealth { get; private set; } = 7;

		[DataMember]
		public decimal StrengthMultiplier { get; set; } = 1;

		[DataMember]
		public decimal AgilityMultiplier { get; set; } = 1;

		[DataMember]
		public decimal IntelligenceMultiplier { get; set; } = 1;

		[DataMember]
		[JsonConverter(typeof(IdentifierBasedConverter<CharacterClass>), typeof(CharacterClassMapper))]
		public CharacterClass CharacterClass { get; set; }

		[DataMember]
		public CharacterColour Colour { get; set; }

		[DataMember]
		public Character NextCardMustTarget { get; set; }

		public decimal CurrentStrength => BaseStrength * StrengthMultiplier;
		public decimal CurrentAgility => BaseAgility * AgilityMultiplier;
		public decimal CurrentIntelligence => BaseIntelligence * IntelligenceMultiplier;

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
		public Dictionary<Category, int> CategoryLevels { get; } = new Dictionary<Category, int>();

		[DataMember]
		public Dictionary<Category, int> AdditionalCategoryLevels { get; } = new Dictionary<Category, int>();

		[DataMember]
		[JsonConverter(typeof(IdentifierBasedConverter<Card>), typeof(CardMapper))]
		public List<Card> Deck { get; private set; } = new List<Card>();

		[DataMember]
		public string OwnerPlayerId { get; set; }

		public bool HasRemainingPlayableCards => RemainingPlayableCards > 0;
		public bool HasRemainingMoves => RemainingMoves > 0;

		public void InitializeDefaultDeck()
		{
			Deck = CardManager.Instance.CreateDefaultDeck(this);
		}

		public void InitializeDeckForBattle()
		{
			Shuffle(Deck);
			DrawOneCard();
			DrawOneCard();
			DrawOneCard();
			DrawOneCard();
		}

		public bool PlayCard(Card card, Character targetCharacter, int roundNumber)
		{
			// Try to play the card, cancelling if it returns false
			if (!card.Play(roundNumber, targetCharacter))
				return false;

			RemainingPlayableCards--;
			card.Discard();
			DrawOneCard();

			card.Categories.ForEach(cardCategory =>
			{
				if (AdditionalCategoryLevels.ContainsKey(cardCategory))
					AdditionalCategoryLevels[cardCategory]++;
				else
					AdditionalCategoryLevels.Add(cardCategory, 1);
			});

			return true;
		}

		private void Shuffle(List<Card> list)
		{
			var n = list.Count;
			while (n > 1)
			{
				n--;
				var k = rng.Next(n + 1);
				var value = list[k];
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

		public IEnumerable<Card> CurrentHand => Deck.Where(card => card.InHand);

		public int CardsInDeckCount => Deck.Count(card => !card.InHand && !card.InDiscard);

		public int CardsInDiscardCount => Deck.Count(card => card.InDiscard);

		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context)
		{
			// We don't serialize the OwnerCharacter for cards as it's wasteful, need to set after we build the Character and Deck
			Deck.ForEach(card =>
			{
				card.OwnerCharacter = this;

				if (card.Condition != null)
					card.Condition.SourceCharacter = this;
			});
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

		public void DrawLock(SpriteBatch spriteBatch)
		{
			var drawPosition = MapPosition.GetScreenPosition();
			drawPosition.Y -= Sprite.Height - (lockSprite.Height / 2);
			drawPosition.X -= lockSprite.Width / 2;

			spriteBatch.Draw(lockSprite, drawPosition, Color.White);
		}

		public void DrawMovementRange(SpriteBatch spriteBatch)
		{
			MapHelper.DrawRange(RemainingMoves, MapPosition, spriteBatch, availableMovementTexture, Color.White, showUnderCharacters: false);
		}
	}
}
