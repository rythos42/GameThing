using System;
using System.Collections.Generic;
using System.Linq;
using GameThing.Data;
using GameThing.Entities;
using GameThing.Entities.Cards;
using GameThing.Events;
using GameThing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace GameThing.Screens
{
	public class BattleScreen : UIEventContainer
	{
		private TiledMapRenderer mapRenderer;
		private Camera<Vector2> camera;
		private RenderTarget2D renderTarget;

		private BattleData data;
		private Character selectedCharacter;
		private readonly HandOfCards handOfCards = new HandOfCards();
		private Card selectedCard;
		private CharacterSide thisPlayerSide;
		private Character lockedInCharacter = null;
		private MapPoint movingPoint;
		private MapPoint targetingPoint;
		private readonly CardManager cardManager;

		private readonly Button newTurnButton;
		private readonly Button winGameNowButton;
		private readonly FadingTextPanel statusPanel = new FadingTextPanel() { PlaceFromRight = true };
		private readonly Panel playerSidePanel = new Panel();
		private readonly Text playerSideText = new Text();

		private readonly Panel gameLogPanel = new Panel();
		private bool showGameLogEntryPanel;
		private Vector2 holdGestureLocation;
		private readonly Panel heldGameLogEntryPanel = new Panel();
		private readonly Text heldGameLogSource = new Text();
		private readonly Text heldGameLogTarget = new Text();
		private readonly Text heldGameLogActionText = new Text();

		private readonly Panel selectedPlayerStatsPanel = new Panel();
		private readonly Text sideText = new Text();
		private readonly Text playerClassText = new Text();
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
		private readonly Random random = new Random();

		public event NextPlayersTurnEventHandler NextPlayersTurn;
		public event GameOverEventHandler GameOver;

		public BattleScreen(CardManager cardManager)
		{
			newTurnButton = new Button("New Turn") { UseMinimumButtonSize = false, Tapped = newTurnButton_Tapped };
			Components.Add(newTurnButton);

			winGameNowButton = new Button("Win Game") { UseMinimumButtonSize = false, Tapped = winGameButton_Tapped };
			Components.Add(winGameNowButton);

			this.cardManager = cardManager;
		}
		public bool IsTestMode { get; set; }

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

			SetGameLogEntryRows();

			// Start next round if all characters activated
			var anyNotActivated = data.Characters.Any(character => !character.ActivatedThisRound);
			if (!anyNotActivated)
				StartNextRound();

			// Show NEW ROUND for first two players, have to recalculate count after StartNextRound or it won't show NEW ROUND
			var countOfActivatedPlayers = data.Characters.Count(character => character.ActivatedThisRound);
			if (countOfActivatedPlayers <= 2)
				statusPanel.Show("NEW ROUND");
		}

		private void SetGameLogEntryRows()
		{
			// Set game log entries from data into the UI in reverse order
			for (int i = data.GameLog.Count - 1, j = 0; i >= 0 && j < gameLogPanel.Components.Count; i--, j++)
				((GameLogEntryRow) gameLogPanel.Components[j]).GameLogEntry = data.GameLog[i];
		}

		private void PlaceCharacterOnMap(Character character, List<MapPoint> otherPlacements)
		{
			MapPoint characterPoint;
			var deployment = character.Side == CharacterSide.Spaghetti ? spaghettiDeployment : unicornDeployment;
			bool yes = character.Side == CharacterSide.Spaghetti;
			do
			{
				characterPoint = new MapPoint(
					random.Next(new Range<int>(deployment.X, deployment.X + deployment.Width)),
					random.Next(new Range<int>(deployment.Y, deployment.Y + deployment.Height))
				);
			} while (otherPlacements.Contains(characterPoint));
			otherPlacements.Add(characterPoint);
			character.MapPosition = characterPoint;
		}

		public override void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			base.LoadContent(content, graphicsDevice);

			var map = content.Map;
			var deploymentLayer = map.GetLayer<TiledMapObjectLayer>("Deployment");
			spaghettiDeployment = MapHelper.GetObjectRectangleInMapPoints(deploymentLayer.Objects.SingleOrDefault(deployment => deployment.Name.Equals("Spaghetti")));
			unicornDeployment = MapHelper.GetObjectRectangleInMapPoints(deploymentLayer.Objects.SingleOrDefault(deployment => deployment.Name.Equals("Unicorn")));
			MapHelper.Map = map;

			mapRenderer = new TiledMapRenderer(graphicsDevice, map);
			camera = new OrthographicCamera(graphicsDevice);
			camera.LookAt(new Vector2(0, map.HeightInPixels / 2));  // center the map in the screen

			selectedPlayerStatsPanel.Components.Add(playerClassText);
			selectedPlayerStatsPanel.Components.Add(sideText);
			selectedPlayerStatsPanel.Components.Add(healthText);
			selectedPlayerStatsPanel.Components.Add(strengthText);
			selectedPlayerStatsPanel.Components.Add(agilityText);
			selectedPlayerStatsPanel.Components.Add(intelligenceText);
			selectedPlayerStatsPanel.Components.Add(remainingDeckText);
			selectedPlayerStatsPanel.Components.Add(discardDeckText);
			selectedPlayerStatsPanel.Components.Add(remainingMovesText);
			selectedPlayerStatsPanel.Components.Add(remainingPlayableCardsText);

			playerSidePanel.Components.Add(playerSideText);

			for (int i = 0; i < 10; i++)
				gameLogPanel.Components.Add(new GameLogEntryRow());

			heldGameLogEntryPanel.Components.Add(heldGameLogSource);
			heldGameLogEntryPanel.Components.Add(heldGameLogTarget);
			heldGameLogEntryPanel.Components.Add(heldGameLogActionText);

			this.content = content;
			handOfCards.Content = content;
			statusPanel.LoadContent(content, graphicsDevice);
			selectedPlayerStatsPanel.LoadContent(content, graphicsDevice);
			playerSidePanel.LoadContent(content, graphicsDevice);
			gameLogPanel.LoadContent(content, graphicsDevice);
			heldGameLogEntryPanel.LoadContent(content, graphicsDevice);

			renderTarget = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
		}

		public void StartGame(string myParticipantId)
		{
			StartGame(data.Sides[myParticipantId]);
		}

		public void StartGame(CharacterSide side)
		{
			thisPlayerSide = side;
			playerSideText.Value = $"Your side: {thisPlayerSide}";
			lockedInCharacter = null;
			selectedCharacter = null;
			selectedCard = null;
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

			// swap device player side if we're in test mode
			if (IsTestMode)
				thisPlayerSide = data.CurrentSidesTurn;
		}

		private void CheckForWin()
		{
			var anySpaghetti = data.Characters.Any(character => character.Side == CharacterSide.Spaghetti);
			var anyUnicorn = data.Characters.Any(character => character.Side == CharacterSide.Unicorn);

			if (!anySpaghetti || !anyUnicorn)
			{
				if (!anySpaghetti && anyUnicorn)
					data.SetWinnerSide(CharacterSide.Unicorn);
				else if (anySpaghetti && !anyUnicorn)
					data.SetWinnerSide(CharacterSide.Spaghetti);

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
				showGameLogEntryPanel = false;

				if (gesture.GestureType == GestureType.FreeDrag)
					Pan(gesture);
				if (gesture.GestureType == GestureType.Pinch)
					Zoom(gesture);
				if (gesture.GestureType == GestureType.Tap)
				{
					InvokeContainerTap(gesture);
					Tap(gesture);
				}
				if (gesture.GestureType == GestureType.Hold)
					Hold(gesture);
			}

			statusPanel.Update(gameTime);

			newTurnButton.IsHighlighted = lockedInCharacter != null && !lockedInCharacter.HasRemainingMoves && !lockedInCharacter.HasRemainingPlayableCards;
		}

		public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Rectangle clientBounds, GameTime gameTime)
		{
			graphicsDevice.SetRenderTarget(renderTarget);
			graphicsDevice.Clear(Color.CornflowerBlue);

			spriteBatch.Begin(
				transformMatrix: camera.GetViewMatrix(),
				samplerState: SamplerState.PointClamp,
				blendState: BlendState.AlphaBlend,
				sortMode: SpriteSortMode.Immediate);
			mapRenderer.Draw(camera.GetViewMatrix());
			spriteBatch.End();

			spriteBatch.Begin(
				transformMatrix: camera.GetViewMatrix(),
				samplerState: SamplerState.PointClamp,
				blendState: BlendState.AlphaBlend,
				sortMode: SpriteSortMode.Texture);
			if (selectedCharacter != null && selectedCard == null && movingPoint == null)
			{
				data.Characters.SingleOrDefault(character => character == selectedCharacter && character.HasRemainingMoves && character.Side == thisPlayerSide)?.DrawMovementRange(spriteBatch);
			}
			else if (selectedCard != null && targetingPoint == null)
			{
				selectedCard.DrawEffectRange(spriteBatch);
			}
			else if (movingPoint != null)
			{
				var movingPointScreen = movingPoint.GetScreenPosition();
				var distanceOverlay = content.DistanceOverlay;
				spriteBatch.Draw(distanceOverlay, new Rectangle((int) movingPointScreen.X - MapPoint.TileWidth_Half, (int) movingPointScreen.Y, distanceOverlay.Width, distanceOverlay.Height), Color.DarkGreen * 0.5f);
			}
			else if (targetingPoint != null)
			{
				var targetingPointScreen = targetingPoint.GetScreenPosition();
				var distanceOverlay = content.DistanceOverlay;
				spriteBatch.Draw(distanceOverlay, new Rectangle((int) targetingPointScreen.X - MapPoint.TileWidth_Half, (int) targetingPointScreen.Y, distanceOverlay.Width, distanceOverlay.Height), Color.DarkBlue * 0.5f);
			}
			spriteBatch.End();

			Effect currentEffect = null;
			bool startedSpriteBatch = false;
			Func<Character, Effect> effectForCharacter = character =>
			{
				if (character == selectedCharacter)
					return content.Highlight;
				if (character.ActivatedThisRound)
					return content.Shade;
				return null;
			};

			data.Characters.Sort(new CharacterDepthComparer());
			foreach (var character in data.Characters)
			{
				var characterEffect = effectForCharacter(character);
				if (!startedSpriteBatch || currentEffect != characterEffect)
				{
					if (startedSpriteBatch)
						spriteBatch.End();

					spriteBatch.Begin(
						transformMatrix: camera.GetViewMatrix(),
						samplerState: SamplerState.PointClamp,
						blendState: BlendState.AlphaBlend,
						effect: characterEffect);
					currentEffect = characterEffect;
					startedSpriteBatch = true;
				}

				if (character == lockedInCharacter)
					character.DrawLock(spriteBatch);

				character.Draw(spriteBatch);
			}
			spriteBatch.End();

			// Draw a characters hand of cards if player is playing that side
			if (selectedCharacter != null && selectedCharacter.Side == thisPlayerSide && thisPlayerSide == data.CurrentSidesTurn)
				handOfCards.Draw(spriteBatch, clientBounds, selectedCharacter.CurrentHand, selectedCard);

			// Draw UI
			spriteBatch.Begin();
			if (thisPlayerSide == data.CurrentSidesTurn)
				newTurnButton.Draw(spriteBatch, UIComponent.MARGIN, UIComponent.MARGIN);
			statusPanel.Draw(spriteBatch, UIComponent.MARGIN, UIComponent.MARGIN);
			playerSidePanel.Draw(spriteBatch, newTurnButton.Width + 2 * UIComponent.MARGIN, UIComponent.MARGIN);
			if (IsTestMode)
				winGameNowButton.Draw(spriteBatch, newTurnButton.Width + playerSidePanel.Width + 3 * UIComponent.MARGIN, UIComponent.MARGIN);

			gameLogPanel.Draw(spriteBatch, UIComponent.MARGIN, newTurnButton.Height + 2 * UIComponent.MARGIN);

			if (selectedCharacter != null)
			{
				playerClassText.Value = $"Class: {selectedCharacter.CharacterClass}";
				sideText.Value = $"Side: {selectedCharacter.Side}";
				healthText.Value = $"Health: {selectedCharacter.CurrentHealth}/{selectedCharacter.CurrentMaxHealth}";
				strengthText.Value = $"Strength: {selectedCharacter.CurrentStrength}/{selectedCharacter.BaseStrength}";
				agilityText.Value = $"Agility: {selectedCharacter.CurrentAgility}/{selectedCharacter.BaseAgility}";
				intelligenceText.Value = $"Intelligence: {selectedCharacter.CurrentIntelligence}/{selectedCharacter.BaseIntelligence}";
				remainingDeckText.Value = $"Cards in Deck: {selectedCharacter.CardsInDeckCount}";
				discardDeckText.Value = $"Cards in Discard: {selectedCharacter.CardsInDiscardCount}";
				remainingMovesText.Value = $"Remaining Moves: {selectedCharacter.RemainingMoves}/{selectedCharacter.MaximumMoves}";
				remainingPlayableCardsText.Value = $"Remaining Plays: {selectedCharacter.RemainingPlayableCards}/{selectedCharacter.MaximumPlayableCards}";
				selectedPlayerStatsPanel.Draw(spriteBatch, gameLogPanel.Width + 2 * UIComponent.MARGIN, newTurnButton.Height + 2 * UIComponent.MARGIN);
			}

			if (showGameLogEntryPanel)
				heldGameLogEntryPanel.Draw(spriteBatch, holdGestureLocation);

			spriteBatch.End();

			graphicsDevice.SetRenderTarget(null);
			spriteBatch.Begin();
			spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
			spriteBatch.End();
		}

		public void newTurnButton_Tapped(GestureSample gesture)
		{
			NextPlayerTurn();
		}

		public void winGameButton_Tapped(GestureSample gesture)
		{
			data.SetWinnerSide(thisPlayerSide);
			GameOver?.Invoke(data);
		}

		private void Tap(GestureSample gesture)
		{
			// Can't tap if it isn't your turn.
			if (data.CurrentSidesTurn != thisPlayerSide)
				return;

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
			else if (selectedCard != null && targetCharacter != null && selectedCard.IsWithinRangeDistance(mapPoint) && mapPoint.IsWithinMap && (lockedInCharacter == selectedCard.OwnerCharacter || lockedInCharacter == null) && targetingPoint == null)
			{
				// TRYING TO TARGET A CARD
				targetingPoint = mapPoint;
			}
			else if (targetingPoint?.Equals(mapPoint) == true)
			{
				// ACTUALLY PLAY SELECTED CARD
				// we have a character selected and a card selected, try to target whatever is under the tap
				if (selectedCharacter.NextCardMustTarget == null || selectedCharacter.NextCardMustTarget == targetCharacter)
				{
					var success = selectedCharacter.PlayCard(selectedCard, targetCharacter, data.RoundNumber);
					if (success)
					{
						if (targetCharacter.CurrentHealth < 1)
						{
							data.Characters.Remove(targetCharacter);
							CheckForWin();
						}

						AddNewGameLogEntry(new GameLogEntry
						{
							SourceCharacter = selectedCharacter,
							TargetCharacter = targetCharacter,
							CardId = selectedCard.Id
						});

						lockedInCharacter = selectedCard.OwnerCharacter;
						selectedCard = null;
						targetingPoint = null;
					}
				}
			}
			else if (selectedCard != null)
			{
				// DESELECTED CARD
				// we have a character selected and a card selected but the card isn't in range of the desired target, or there is no target
				selectedCard = null;
				targetingPoint = null;
			}
			else if (selectedCharacter.IsWithinMoveDistanceOf(mapPoint) && mapPoint.IsWithinMap && mapPoint.IsInAvailableMovement && CanMoveSelectedCharacter && targetCharacter == null && movingPoint == null)
			{
				// TRYING TO MOVE
				// we have a character selected, it's my character, it is within move distance of the tap and it has moves remaining
				movingPoint = mapPoint;
			}
			else if (movingPoint?.Equals(mapPoint) == true)
			{
				// ACTUALLY MOVING
				selectedCharacter.Move(mapPoint);
				lockedInCharacter = selectedCharacter;
				movingPoint = null;
				AddNewGameLogEntry(new GameLogEntry
				{
					SourceCharacter = selectedCharacter,
					MovedTo = mapPoint
				});
			}
			else
			{
				// DESELECT CHARACTER AND CARD
				// we have a character selected, but didn't tap anything else useful so unselect it
				selectedCharacter = null;
				selectedCard = null;
				movingPoint = null;
				targetingPoint = null;
				handOfCards.ClearCardPositions();
			}
		}

		private void AddNewGameLogEntry(GameLogEntry entry)
		{
			data.GameLog.Add(entry);
			SetGameLogEntryRows();
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

		private void Hold(GestureSample gesture)
		{
			var heldLogEntryRow = gameLogPanel.Components.FirstOrDefault(component => component.IsAtPoint(gesture.Position)) as GameLogEntryRow;
			if (heldLogEntryRow == null)
				return;

			showGameLogEntryPanel = true;
			holdGestureLocation = gesture.Position;
			var entry = heldLogEntryRow.GameLogEntry;
			heldGameLogSource.Value = $"Source: {entry.SourceCharacterColour} on {entry.SourceCharacterSide}";

			if (entry.MovedTo != null)
			{
				heldGameLogTarget.Value = null;
				heldGameLogActionText.Value = $"Moved to ({entry.MovedTo.X}, {entry.MovedTo.Y})";
			}
			else
			{
				heldGameLogTarget.Value = $"Target:  {entry.TargetCharacterColour} on {entry.TargetCharacterSide}";
				heldGameLogActionText.Value = cardManager.GetCard(entry.CardId).Description;
			}
		}
	}
}
