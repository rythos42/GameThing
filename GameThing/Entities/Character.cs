using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GameThing.Contract;
using GameThing.Database;
using GameThing.Entities.Cards;
using GameThing.Entities.Cards.Conditions;
using GameThing.Entities.Cards.Conditions.Recurrence;
using GameThing.Manager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using Newtonsoft.Json;

namespace GameThing.Entities
{
	[DataContract]
	public class Character : Drawable
	{
		private Texture2D availableMovementTexture;
		private Texture2D lockSprite;
		private readonly Dictionary<CharacterFacing, AnimatedSprite> animatedSprite = new Dictionary<CharacterFacing, AnimatedSprite>();

		private const double EvadeConstant = 0.025;         // Used in Evade quadratic equation a*x^2
		private const decimal LastFirstPlayDebuff = 0.5m;   // How much a character is debuffed for damage or healing if they go last in a turn, then go first in a turn.

		public Character(Guid id, CharacterColour colour, CharacterClass characterClass)
		{
			Id = id;
			CharacterClass = characterClass;
			Colour = colour;
		}

		public void SetContent(Content content, CharacterSide side)
		{
			availableMovementTexture = content.DistanceOverlay;
			lockSprite = content.Lock;

			var animationFactory = content.GetAnimationFactory(side);
			animatedSprite[CharacterFacing.North] = new AnimatedSprite(animationFactory, content.GetSpriteTag(Colour, CharacterFacing.North));
			animatedSprite[CharacterFacing.East] = new AnimatedSprite(animationFactory, content.GetSpriteTag(Colour, CharacterFacing.East));
			animatedSprite[CharacterFacing.South] = new AnimatedSprite(animationFactory, content.GetSpriteTag(Colour, CharacterFacing.South));
			animatedSprite[CharacterFacing.West] = new AnimatedSprite(animationFactory, content.GetSpriteTag(Colour, CharacterFacing.West));
		}

		public static IRandomWrapper Random { get; internal set; } = new RandomWrapper();

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public override MapPoint MapPosition { get; set; }

		public override int SpriteHeight { get { return 170; } }
		public override int SpriteWidth { get { return 85; } }

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

		public void ApplyDamage(decimal damageAmount)
		{
			baseAbilityScores[AbilityScore.Health] -= damageAmount;
		}

		public PlayStatus AttemptToApplyDamage(decimal damageAmount)
		{
			var currentEvade = GetCurrentAbilityScore(AbilityScore.Evade);
			var missChance = EvadeConstant * (double) (currentEvade * currentEvade);

			if (Random.NextDouble() <= missChance)
				return new PlayStatus(PlayStatusDetails.FailedEvaded);

			var actualDamageDone = damageAmount * DefenseDamageMultipler;
			baseAbilityScores[AbilityScore.Health] -= actualDamageDone;
			return new PlayStatus(PlayStatusDetails.Success) { ActualDamageOrHealingDone = actualDamageDone, CardType = CardType.Damage };
		}

		private decimal DefenseDamageMultipler { get { return 1 - ((GetCurrentAbilityScore(AbilityScore.Defense) - 1) * 0.1m); } }

		public PlayStatus ApplyHealing(decimal healAmount)
		{
			var actualHealingDone = baseAbilityScores[AbilityScore.Health] + healAmount;
			baseAbilityScores[AbilityScore.Health] = Math.Min(CurrentMaxHealth, actualHealingDone);
			return new PlayStatus(PlayStatusDetails.Success) { ActualDamageOrHealingDone = actualHealingDone, CardType = CardType.Heal };
		}

		public decimal ChangeDamageOrHealingForStamina(decimal damageOrHealAmount, int turnNumber)
		{
			// Do nothing if the character hasn't gone yet.
			if (LastPlayedTurnNumber == -1)
				return damageOrHealAmount;

			var initiativeModifierFromStamina = 2m - (1m / GetCurrentAbilityScore(AbilityScore.Stamina));
			var turnsSinceLastPlayed = turnNumber - LastPlayedTurnNumber - 1;               // determine how many characters have gone since this characters last turn
			var unitChunk = (1 - LastFirstPlayDebuff) / CharactersInGameCount;              // determine how much bonus you get for each character that has gone since your last turn
			var initiativeDebuff = turnsSinceLastPlayed * unitChunk + LastFirstPlayDebuff;  // if you go right away again, 0.5 of the total effect is applied. Otherwise, add how much you get from waiting

			return Math.Min(initiativeDebuff * initiativeModifierFromStamina * damageOrHealAmount, damageOrHealAmount);  // Can't go above what was asked for.
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
					condition.Condition.RemoveEffects(this);
					Conditions.Remove(condition);
				}
			}
		}

		private void ApplyRecurrence(RecurrencePeriod period, RecurrenceTrigger trigger)
		{
			foreach (var condition in Conditions)
			{
				if (condition.Condition.Recurrence.Is(period, trigger))
					condition.Condition.ApplyEffects(null, this);
			}
		}

		public void StartNewRound(int roundNumber)
		{
			RemoveConditions(ConditionEndsOn.StartRound, condition => condition.RoundNumber + (condition.Condition.TurnCount ?? 0) == roundNumber);

			ResetTurn();
		}

		public void EndRound(int roundNumber)
		{
			ApplyRecurrence(RecurrencePeriod.PerRound, RecurrenceTrigger.End);

			RemoveConditions(ConditionEndsOn.EndRound, condition => condition.RoundNumber + (condition.Condition.TurnCount ?? 0) == roundNumber);
		}

		public void EndTurn(int thisTurnNumber)
		{
			LastPlayedTurnNumber = thisTurnNumber;
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
		public CharacterFacing Facing { get; set; } = CharacterFacing.South;

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

		[DataMember]
		public int LastPlayedTurnNumber { get; private set; } = -1;
		public int CharactersInGameCount { get; set; }

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

		public PlayStatus PlayCard(Card card, Character targetCharacter, int roundNumber, int turnNumber)
		{
			if (NextCardMustTarget != null && NextCardMustTarget != targetCharacter)
				return new PlayStatus(PlayStatusDetails.FailedTaunted) { PlayCancelled = true };

			// Try to play the card
			var played = card.Play(targetCharacter, roundNumber, turnNumber);

			// Don't want to penalize player for these failures: act as though card isn't played
			if (played.PlayCancelled)
				return played;

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

			return played;
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
			// We don't serialize the OwningCharacter for cards as it's wasteful, need to set after we build the Character and Deck
			Deck.ForEach(card =>
			{
				card.OwnerCharacter = this;

				if (card.Condition != null)
					card.Condition.OwningCharacter = this;
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
			var xDistance = MapPosition.X - movePoint.X;
			var yDistance = MapPosition.Y - movePoint.Y;
			var xAbsDistance = Math.Abs(xDistance);
			var yAbsDistance = Math.Abs(yDistance);
			var distance = xAbsDistance + yAbsDistance;

			RemainingMoves -= distance;
			MapPosition = movePoint;

			RemoveConditions(ConditionEndsOn.Move);

			// EQUAL in first condition means prefer to set facing to North/South when moving exactly diagonally
			// Ignore EQUAL in second conditions because character's can't move 0 squares, so have to be moving along an axis if 1 axis is greater than the other
			Facing = yAbsDistance >= xAbsDistance
				? yDistance < 0 ? CharacterFacing.South : CharacterFacing.North
				: xDistance < 0 ? CharacterFacing.East : CharacterFacing.West;

		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var drawPosition = MapPosition.GetScreenPosition();
			drawPosition.Y -= MapPoint.TileHeight_Half + MapPoint.TileHeight; // this calculation may not be related to these values, not sure whats up here
			drawPosition.X += 5;    // just a little bump

			spriteBatch.Draw(animatedSprite[Facing], drawPosition);
		}

		public void DrawLock(SpriteBatch spriteBatch)
		{
			var drawPosition = MapPosition.GetScreenPosition();
			drawPosition.Y -= SpriteHeight - (lockSprite.Height / 2);
			drawPosition.X -= lockSprite.Width / 2;

			spriteBatch.Draw(lockSprite, drawPosition, Color.White);
		}

		public void DrawMovementRange(SpriteBatch spriteBatch)
		{
			MapHelper.DrawRange(RemainingMoves, MapPosition, spriteBatch, availableMovementTexture, Color.White, showUnderCharacters: false);
		}

		public override void Update(GameTime gameTime)
		{
			animatedSprite[Facing].Update(gameTime);
		}
	}
}
