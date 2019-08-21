using System;
using System.Collections.Generic;
using System.Linq;
using GameThing.Entities;
using GameThing.Entities.Cards;
using GameThing.Events;
using GameThing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace GameThing.Screens
{
	public class BattleScreen
	{
		private TiledMap map;
		private TiledMapRenderer mapRenderer;
		private Camera<Vector2> camera;

		private BattleData data;
		private Character selectedCharacter;
		private HandOfCards handOfCards = new HandOfCards();
		private Card selectedCard;
		private CharacterSide thisPlayerSide;
		private Character lockedInCharacter = null;

		private Button newTurnButton = new Button("New Turn") { UseMinimumButtonSize = false };
		private FadingTextPanel statusPanel = new FadingTextPanel() { PlaceFromRight = true };
		private Panel playerSidePanel = new Panel();
		private Panel selectedPlayerStatsPanel = new Panel();
		private readonly Text sideText = new Text();
		private readonly Text healthText = new Text();
		private readonly Text strengthText = new Text();
		private readonly Text agilityText = new Text();
		private readonly Text intelligenceText = new Text();
		private readonly Text remainingDeckText = new Text();
		private readonly Text discardDeckText = new Text();
		private readonly Text remainingMovesText = new Text();
		private readonly Text remainingPlayableCardsText = new Text();

		private Rectangle spaghettiDeployment;
		private Rectangle unicornDeployment;

		private Content content;
		private Random random = new Random();

		public event NextPlayersTurnEventHandler NextPlayersTurn;
		public event GameOverEventHandler GameOver;

		public void SetBattleData(BattleData gameData)
		{
			var otherPlacements = new List<MapPoint>();

			gameData.Characters.ForEach(character =>
			{
				character.SetContent(content);
				character.Deck.ForEach(card => card.SetContent(content));

				if (character.MapPosition == null)
					PlaceCharacterOnMap(character, otherPlacements);
			});
			data = gameData;

			// Start next round if all characters activated
			var anyNotActivated = data.Characters.Any(character => !character.ActivatedThisRound);
			if (!anyNotActivated)
				StartNextRound();

			// Show NEW ROUND for first two players, have to recalculate count after StartNextRound or it won't show NEW ROUND
			var countOfActivatedPlayers = data.Characters.Count(character => character.ActivatedThisRound);
			if (countOfActivatedPlayers <= 2)
				statusPanel.Show("NEW ROUND");
		}

		private void PlaceCharacterOnMap(Character character, List<MapPoint> otherPlacements)
		{
			MapPoint characterPoint;
			var deployment = character.Side == CharacterSide.Spaghetti ? spaghettiDeployment : unicornDeployment;
			bool yes = character.Side == CharacterSide.Spaghetti;
			do
			{
				characterPoint = new MapPoint
				{
					X = random.Next(new Range<int>(deployment.X, deployment.X + deployment.Width)),
					Y = random.Next(new Range<int>(deployment.Y, deployment.Y + deployment.Height))
				};
			} while (otherPlacements.Contains(characterPoint));
			otherPlacements.Add(characterPoint);
			character.MapPosition = characterPoint;
		}

		private Rectangle GetDeployment(TiledMapObject deployment)
		{
			return new Rectangle(
				(int) Math.Round(deployment.Position.X / 32, 0, MidpointRounding.AwayFromZero),
				(int) Math.Round(deployment.Position.Y / 32, 0, MidpointRounding.AwayFromZero),
				(int) Math.Round(deployment.Size.Width / 32, 0, MidpointRounding.AwayFromZero),
				(int) Math.Round(deployment.Size.Height / 32, 0, MidpointRounding.AwayFromZero));
		}

		public void LoadContent(Content content, ContentManager contentManager, GraphicsDevice graphicsDevice)
		{
			//map = contentManager.Load<TiledMap>("tilemaps/Map");
			map = contentManager.Load<TiledMap>("tilemaps/ComplexMap");
			var deploymentLayer = map.GetLayer<TiledMapObjectLayer>("Deployment");
			spaghettiDeployment = GetDeployment(deploymentLayer.Objects.SingleOrDefault(deployment => deployment.Name.Equals("Spaghetti")));
			unicornDeployment = GetDeployment(deploymentLayer.Objects.SingleOrDefault(deployment => deployment.Name.Equals("Unicorn")));

			mapRenderer = new TiledMapRenderer(graphicsDevice, map);
			camera = new OrthographicCamera(graphicsDevice);
			camera.LookAt(new Vector2(0, map.HeightInPixels / 2));  // center the map in the screen

			selectedPlayerStatsPanel.Components.Add(sideText);
			selectedPlayerStatsPanel.Components.Add(healthText);
			selectedPlayerStatsPanel.Components.Add(strengthText);
			selectedPlayerStatsPanel.Components.Add(agilityText);
			selectedPlayerStatsPanel.Components.Add(intelligenceText);
			selectedPlayerStatsPanel.Components.Add(remainingDeckText);
			selectedPlayerStatsPanel.Components.Add(discardDeckText);
			selectedPlayerStatsPanel.Components.Add(remainingMovesText);
			selectedPlayerStatsPanel.Components.Add(remainingPlayableCardsText);

			playerSidePanel.Components.Add(new Text { Value = $"Your side: {thisPlayerSide}" });

			this.content = content;
			newTurnButton.LoadContent(content, contentManager, graphicsDevice);
			statusPanel.LoadContent(content, contentManager, graphicsDevice);
			selectedPlayerStatsPanel.LoadContent(content, contentManager, graphicsDevice);
			playerSidePanel.LoadContent(content, contentManager, graphicsDevice);
		}

		public void StartGame(string myParticipantId)
		{
			thisPlayerSide = data.Sides[myParticipantId];
		}

		public void StartGame(CharacterSide side)
		{
			thisPlayerSide = side;
			lockedInCharacter = null;
		}

		private void StartNextRound()
		{
			data.RoundNumber++;
			data.Characters.ForEach(character => character.StartNewRound(data.RoundNumber));
			lockedInCharacter = null;
		}

		private void NextPlayerTurn()
		{
			if (lockedInCharacter == null)
			{
				statusPanel.Show("You must activate a character this turn.");
				return;
			}

			lockedInCharacter?.EndTurn();
			lockedInCharacter = null;

			if (data.OtherSideHasNoRemainingCharactersAndIHaveSome)
			{
				statusPanel.Show("Opponent has no more characters to activate, go again!");
				return;
			}

			data.ChangePlayingSide();
			NextPlayersTurn?.Invoke(data);
		}

		private void CheckForWin()
		{
			var anySpaghetti = data.Characters.Any(character => character.Side == CharacterSide.Spaghetti);
			var anyUnicorn = data.Characters.Any(character => character.Side == CharacterSide.Unicorn);

			if (!anySpaghetti || !anyUnicorn)
			{
				if (!anySpaghetti && anyUnicorn)
					data.Winner = CharacterSide.Unicorn;
				else if (anySpaghetti && !anyUnicorn)
					data.Winner = CharacterSide.Spaghetti;

				GameOver?.Invoke(data);
			}
		}

		public void Update(GameTime gameTime)
		{
			mapRenderer.Update(gameTime);

			var gesture = default(GestureSample);

			while (TouchPanel.IsGestureAvailable)
			{
				gesture = TouchPanel.ReadGesture();

				if (gesture.GestureType == GestureType.FreeDrag)
					Pan(gesture);
				if (gesture.GestureType == GestureType.Pinch)
					Zoom(gesture);
				if (gesture.GestureType == GestureType.Tap)
					Tap(gesture);
			}

			statusPanel.Update(gameTime);

			newTurnButton.IsHighlighted = lockedInCharacter != null && !lockedInCharacter.HasRemainingMoves && !lockedInCharacter.HasRemainingPlayableCards;
		}

		public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Rectangle clientBounds, GameTime gameTime)
		{
			graphicsDevice.Clear(Color.CornflowerBlue);

			// Follow camera
			spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
			mapRenderer.Draw(camera.GetViewMatrix());
			if (selectedCard == null)
				data.Characters.SingleOrDefault(character => character == selectedCharacter && character.HasRemainingMoves && character.Side == thisPlayerSide)?.DrawMovementRange(spriteBatch);
			else
				selectedCard.DrawEffectRange(spriteBatch);
			data.Characters.Sort(new CharacterDepthComparer());
			foreach (var character in data.Characters)
			{
				if (character == lockedInCharacter)
					character.DrawLock(spriteBatch);

				character.Draw(spriteBatch);
			}
			spriteBatch.End();

			selectedCharacter?.DrawSelectedCharacter(spriteBatch, camera.GetViewMatrix());

			// Draw a characters hand of cards if player is playing that side
			if (selectedCharacter != null && selectedCharacter.Side == thisPlayerSide && thisPlayerSide == data.CurrentSidesTurn)
				handOfCards.Draw(spriteBatch, clientBounds, selectedCharacter.CurrentHand, selectedCard);

			// Draw UI
			spriteBatch.Begin();
			if (thisPlayerSide == data.CurrentSidesTurn)
				newTurnButton.Draw(spriteBatch, 40, 20);
			statusPanel.Draw(spriteBatch, 40, 20);
			playerSidePanel.Draw(spriteBatch, 220, 20);
			if (selectedCharacter != null)
			{
				sideText.Value = $"Side: {selectedCharacter.Side}";
				healthText.Value = $"Health: {selectedCharacter.CurrentHealth}/{selectedCharacter.CurrentMaxHealth}";
				strengthText.Value = $"Strength: {selectedCharacter.CurrentStrength}/{selectedCharacter.BaseStrength}";
				agilityText.Value = $"Agility: {selectedCharacter.CurrentAgility}/{selectedCharacter.BaseAgility}";
				intelligenceText.Value = $"Intelligence: {selectedCharacter.CurrentIntelligence}/{selectedCharacter.BaseIntelligence}";
				remainingDeckText.Value = $"Cards in Deck: {selectedCharacter.CardsInDeckCount}";
				discardDeckText.Value = $"Cards in Discard: {selectedCharacter.CardsInDiscardCount}";
				remainingMovesText.Value = $"Remaining Moves: {selectedCharacter.RemainingMoves}/{selectedCharacter.MaximumMoves}";
				remainingPlayableCardsText.Value = $"Remaining Plays: {selectedCharacter.RemainingPlayableCards}/{selectedCharacter.MaximumPlayableCards}";
				selectedPlayerStatsPanel.Draw(spriteBatch, 40, 110);
			}
			spriteBatch.End();
		}

		private void Tap(GestureSample gesture)
		{
			// Can't tap if it isn't your turn.
			if (data.CurrentSidesTurn != thisPlayerSide)
				return;

			// New Turn Button
			if (newTurnButton.IsAtPoint(gesture.Position))
			{
				NextPlayerTurn();
				return;
			}

			// try to get a card under the tap first 
			var selectedCardIndex = handOfCards.GetCardIndexAtPosition(gesture.Position);
			if (selectedCardIndex != -1)
			{
				// SELECT CARD
				if (CanSelectCardFromSelectedCharacter)
					selectedCard = selectedCharacter.CurrentHand.ElementAt(selectedCardIndex);
				return;
			}

			var worldPoint = camera.ScreenToWorld(gesture.Position);
			var mapPoint = MapPoint.GetFromScreenPosition(worldPoint);
			var targetCharacter = data.Characters.SingleOrDefault(character => character.IsAtPoint(mapPoint));
			if (selectedCharacter == null)
			{
				// SELECT CHARACTER
				// we don't have a character selected, so select whatever we have under the tap
				selectedCharacter = targetCharacter;
			}
			else if (selectedCard != null && targetCharacter != null && selectedCard.IsWithinRangeDistance(mapPoint) && (lockedInCharacter == selectedCard.OwnerCharacter || lockedInCharacter == null))
			{
				// PLAY SELECTED CARD
				// we have a character selected and a card selected, try to target whatever is under the tap
				if (selectedCharacter.NextCardMustTarget == null || selectedCharacter.NextCardMustTarget == targetCharacter)
					selectedCharacter.PlayCard(selectedCard, targetCharacter, data.RoundNumber);

				if (targetCharacter.CurrentHealth < 1)
				{
					data.Characters.Remove(targetCharacter);
					CheckForWin();
				}

				lockedInCharacter = selectedCard.OwnerCharacter;
				selectedCard = null;
			}
			else if (selectedCard != null)
			{
				// DESELECTED CARD
				// we have a character selected and a card selected but the card isn't in range of the desired target, or there is no target
				selectedCard = null;
			}
			else if (selectedCharacter.IsWithinMoveDistanceOf(mapPoint) && mapPoint.IsWithinMap && CanMoveSelectedCharacter && targetCharacter == null)
			{
				// MOVE
				// we have a character selected, it's my character, it is within move distance of the tap and it has moves remaining
				selectedCharacter.Move(mapPoint);
				lockedInCharacter = selectedCharacter;
			}
			else
			{
				// DESELECT CHARACTER AND CARD
				// we have a character selected, but didn't tap anything else useful so unselect it
				selectedCharacter = null;
				selectedCard = null;
				handOfCards.ClearCardPositions();
			}
		}

		private bool CanMoveSelectedCharacter
		{
			get
			{
				return selectedCharacter.Side == thisPlayerSide
					&& selectedCharacter.HasRemainingMoves
					&& !selectedCharacter.ActivatedThisRound
					&& (lockedInCharacter == selectedCharacter || lockedInCharacter == null);
			}
		}

		private bool CanSelectCardFromSelectedCharacter
		{
			get
			{
				return selectedCharacter.HasRemainingPlayableCards && !selectedCharacter.ActivatedThisRound;
			}
		}

		private void Pan(GestureSample gesture)
		{
			var x = -1 * gesture.Delta.X / camera.Zoom;
			var y = -1 * gesture.Delta.Y / camera.Zoom;
			camera.Move(new Vector2(x, y));
		}

		private void Zoom(GestureSample gesture)
		{
			if (gesture.GestureType == GestureType.Pinch)
			{
				Vector2 a = gesture.Position;
				Vector2 b = gesture.Position2;
				float dist = Vector2.Distance(a, b);

				Vector2 aOld = gesture.Position - gesture.Delta;
				Vector2 bOld = gesture.Position2 - gesture.Delta2;
				float distOld = Vector2.Distance(aOld, bOld);

				float scale = (distOld - dist) * -0.05f;
				if (camera.Zoom + scale > 5 || camera.Zoom + scale < 1)
					return;

				camera.Zoom += scale;
			}
		}
	}
}
