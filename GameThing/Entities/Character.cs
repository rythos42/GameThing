﻿using System;
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
		private const double EvadeConstant = 0.025; // Used in Evade quadratic equation a*x^2

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

		public static IRandomWrapper Random { get; internal set; } = new RandomWrapper();

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public override MapPoint MapPosition { get; set; }

		[DataMember]
		private readonly IDictionary<AbilityScore, decimal> baseAbilityScores = new Dictionary<AbilityScore, decimal>
		{
			{ AbilityScore.Strength, 1 },
			{ AbilityScore.Agility, 1 },
			{ AbilityScore.Intelligence, 1 },
			{ AbilityScore.Stamina, 1 },
			{ AbilityScore.Evade, 1 },
			{ AbilityScore.Defense, 1 },
			{ AbilityScore.Health, 7 },
		};

		[DataMember]
		private readonly IDictionary<AbilityScore, decimal> abilityScoreMultipliers = new Dictionary<AbilityScore, decimal>
		{
			{ AbilityScore.Strength, 1 },
			{ AbilityScore.Agility, 1 },
			{ AbilityScore.Intelligence, 1 },
			{ AbilityScore.Stamina, 1 },
			{ AbilityScore.Evade, 1 },
			{ AbilityScore.Defense, 1 },
			{ AbilityScore.Health, 1 },
		};

		public IDictionary<string, decimal> TemplatingBaseAbilityScores => baseAbilityScores.ToDictionary(item => item.Key.ToString(), item => item.Value);
		public IDictionary<string, decimal> TemplatingCurrentAbilityScores => baseAbilityScores.ToDictionary(item => item.Key.ToString(), item => GetCurrentAbilityScore(item.Key));

		public decimal GetBaseAbilityScore(AbilityScore score) => baseAbilityScores[score];
		public decimal GetAbilityScoreMultiplier(AbilityScore score) => abilityScoreMultipliers[score];
		public decimal SetAbilityScoreMultiplier(AbilityScore score, decimal scoreValue) => abilityScoreMultipliers[score] = scoreValue;
		public decimal GetCurrentAbilityScore(AbilityScore score) => baseAbilityScores[score] * abilityScoreMultipliers[score];

		internal void SetCurrentHealth(decimal health)
		{
			baseAbilityScores[AbilityScore.Health] = health;
		}

		[DataMember]
		public decimal CurrentMaxHealth { get; private set; } = 7;

		[DataMember]
		[JsonConverter(typeof(IdentifierBasedConverter<CharacterClass>), typeof(CharacterClassMapper))]
		public CharacterClass CharacterClass { get; set; }

		[DataMember]
		public CharacterColour Colour { get; set; }

		[DataMember]
		public Character NextCardMustTarget { get; set; }

		public void AttemptToApplyDamage(decimal damageAmount)
		{
			var currentEvade = GetCurrentAbilityScore(AbilityScore.Evade);
			var missChance = EvadeConstant * (double) (currentEvade * currentEvade);

			if (Random.NextDouble() > missChance)
				baseAbilityScores[AbilityScore.Health] -= damageAmount * DefenseDamageMultipler;
		}

		private decimal DefenseDamageMultipler { get { return 1 - ((GetCurrentAbilityScore(AbilityScore.Defense) - 1) * 0.1m); } }

		public void ApplyHealing(decimal damageAmount)
		{
			baseAbilityScores[AbilityScore.Health] += damageAmount;
			if (baseAbilityScores[AbilityScore.Health] > CurrentMaxHealth)
				baseAbilityScores[AbilityScore.Health] = CurrentMaxHealth;
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
				var k = Random.Next(n + 1);
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
