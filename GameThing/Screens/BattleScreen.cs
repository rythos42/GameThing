using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameThing.Contract;
using GameThing.Entities;
using GameThing.Entities.Cards;
using GameThing.Manager;
using GameThing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using IDrawable = GameThing.Entities.IDrawable;

namespace GameThing.Screens
{
	public class BattleScreen
	{
		private const int GameLogEntryCount = 10;

		private TiledMapRenderer mapRenderer;
		private Camera<Vector2> camera;
		private RenderTarget2D renderTarget;

		private BattleData data;
		private Character selectedCharacter;
		private readonly HandOfCards handOfCards = new HandOfCards();
		private Card selectedCard;
		private Character lockedInCharacter = null;
		private readonly DrawableList entities = new DrawableList();
		private MapPoint movingPoint;
		private MapPoint targetingPoint;
		private readonly CardManager cardManager = CardManager.Instance;
		private readonly BattleManager battleManager = BattleManager.Instance;

		private ScreenComponent screenComponent;

		private readonly Button newTurnButton;
		private readonly Button winGameNowButton;
		private readonly FadingTextPanel statusPanel = new FadingTextPanel() { PlaceFromRight = true };
		private readonly Panel playerSidePanel = new Panel();
		private readonly Text playerSideText = new Text();

		private readonly Panel gameLogPanel = new Panel();
		private bool showGameLogEntryPanel;
		private readonly Panel heldGameLogEntryPanel = new Panel();
		private readonly Text heldGameLogSource = new Text();
		private readonly Text heldGameLogTarget = new Text();
		private readonly Text heldGameLogActionText = new Text();

		private bool showAppliedConditionDetailsPanel;
		private readonly Panel appliedConditionDetailsPanel = new Panel();
		private readonly Text appliedConditionDetailsText = new Text();
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
		private readonly AppliedConditionRow appliedConditionRow;

		private Rectangle spaghettiDeployment;
		private Rectangle unicornDeployment;

		private Content content;
		private readonly Random random = new Random();

		public event BattleGameOverEventHandler GameOver;
		public event SelectedCharacterChangeEventHandler SelectedCharacterChange;

		public BattleScreen()
		{
			newTurnButton = new Button("New Turn") { UseMinimumButtonSize = false, Tapped = newTurnButton_Tapped };
			winGameNowButton = new Button("Win Game") { UseMinimumButtonSize = false, Tapped = winGameNowButton_Tapped };
			appliedConditionRow = new AppliedConditionRow { Held = appliedConditionRow_Held };


			BattleManager.Instance.DataUpdated += BattleManager_DataUpdated;
		}

		private void BattleManager_DataUpdated(BattleData battleData)
		{
			SetBattleData(battleData);

			if (battleData.Status == BattleStatus.Finished)
				GameOver?.Invoke(battleData);
		}

		public void SetBattleData(BattleData battleData)
		{
			var otherPlacements = new List<MapPoint>();

			battleData.Characters.ForEach(character =>
			{
				var side = battleData.Sides[character.OwnerPlayerId];
				character.SetContent(content, side);
				character.Deck.ForEach(card => card.SetContent(content));

				if (character.MapPosition == null)
					PlaceCharacterOnMap(character, otherPlacements, side);
			});
			data = battleData;
			entities.SetCharacters(battleData.Characters);

			SetGameLogEntryRows();

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

		private void PlaceCharacterOnMap(Character character, List<MapPoint> otherPlacements, CharacterSide side)
		{
			MapPoint characterPoint;
			var deployment = side == CharacterSide.Spaghetti ? spaghettiDeployment : unicornDeployment;
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

		public void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			var map = content.Map;
			var deploymentLayer = map.GetLayer<TiledMapObjectLayer>("Deployment");
			spaghettiDeployment = MapHelper.GetObjectRectangleInMapPoints(deploymentLayer.Objects.SingleOrDefault(deployment => deployment.Name.Equals("Spaghetti")));
			unicornDeployment = MapHelper.GetObjectRectangleInMapPoints(deploymentLayer.Objects.SingleOrDefault(deployment => deployment.Name.Equals("Unicorn")));
			MapHelper.Map = map;
			MapHelper.Entities = entities;

			entities.Add(new Terrain(content.MediumTree, new MapPoint(3, 4)));
			entities.Add(new Terrain(content.MediumTree, new MapPoint(17, 21)));
			entities.Add(new Terrain(content.MediumTree, new MapPoint(23, 2)));
			entities.Add(new Terrain(content.MediumTree, new MapPoint(6, 15)));
			entities.Add(new Terrain(content.SmallTree, new MapPoint(15, 20)));
			entities.Add(new Terrain(content.SmallTree, new MapPoint(19, 22)));
			entities.Add(new Terrain(content.SmallTree, new MapPoint(6, 16)));
			entities.Add(new Terrain(content.SmallTree, new MapPoint(6, 14)));
			entities.Add(new Terrain(content.SmallTree, new MapPoint(28, 20)));
			entities.Add(new Terrain(content.SmallTree, new MapPoint(24, 25)));
			entities.Add(new Terrain(content.LargeBush, new MapPoint(26, 6)));
			entities.Add(new Terrain(content.LargeBush, new MapPoint(16, 10)));
			entities.Add(new Terrain(content.LargeBush, new MapPoint(1, 20)));

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
			selectedPlayerStatsPanel.Components.Add(appliedConditionRow);
			SelectedCharacterChange += UpdateSelectedCharacterPanel;

			playerSidePanel.Components.Add(playerSideText);

			for (int i = 0; i < GameLogEntryCount; i++)
				gameLogPanel.Components.Add(new GameLogEntryRow { Held = gameLogEntryRow_Held });

			heldGameLogEntryPanel.Components.Add(heldGameLogSource);
			heldGameLogEntryPanel.Components.Add(heldGameLogTarget);
			heldGameLogEntryPanel.Components.Add(heldGameLogActionText);

			appliedConditionDetailsPanel.Components.Add(appliedConditionDetailsText);

			screenComponent = new ScreenComponent(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
			screenComponent.GestureRead += screenComponent_GestureRead;
			screenComponent.Components.Add(newTurnButton);
			screenComponent.Components.Add(winGameNowButton);
			screenComponent.Components.Add(selectedPlayerStatsPanel);
			screenComponent.Components.Add(gameLogPanel);
			screenComponent.Components.Add(playerSidePanel);
			screenComponent.Components.Add(heldGameLogEntryPanel);
			screenComponent.Components.Add(appliedConditionDetailsPanel);
			screenComponent.Components.Add(statusPanel);
			screenComponent.LoadContent(content, graphicsDevice);

			this.content = content;
			handOfCards.Content = content;
			winGameNowButton.LoadContent(content, graphicsDevice);

			renderTarget = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
		}

		public void UpdateSelectedCharacterPanel(Character newCharacter)
		{
			if (newCharacter == null)
				return;

			playerClassText.Value = $"Class: {selectedCharacter.CharacterClass.Name}";
			sideText.Value = $"Side: {data.Sides[selectedCharacter.OwnerPlayerId]}";
			healthText.Value = $"Health: {selectedCharacter.CurrentHealth}/{selectedCharacter.CurrentMaxHealth}";
			strengthText.Value = $"Strength: {selectedCharacter.CurrentStrength}/{selectedCharacter.BaseStrength}";
			agilityText.Value = $"Agility: {selectedCharacter.CurrentAgility}/{selectedCharacter.BaseAgility}";
			intelligenceText.Value = $"Intelligence: {selectedCharacter.CurrentIntelligence}/{selectedCharacter.BaseIntelligence}";
			remainingDeckText.Value = $"Cards in Deck: {selectedCharacter.CardsInDeckCount}";
			discardDeckText.Value = $"Cards in Discard: {selectedCharacter.CardsInDiscardCount}";
			remainingMovesText.Value = $"Remaining Moves: {selectedCharacter.RemainingMoves}/{selectedCharacter.MaximumMoves}";
			remainingPlayableCardsText.Value = $"Remaining Plays: {selectedCharacter.RemainingPlayableCards}/{selectedCharacter.MaximumPlayableCards}";
			appliedConditionRow.SelectedCharacter = newCharacter;
		}

		public void StartGame(BattleData data)
		{
			SetBattleData(data);
			playerSideText.Value = $"Your side: {data.Sides[data.GetPlayerId()]}";
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

		private async Task NextPlayerTurn()
		{
			if (lockedInCharacter == null)
			{
				statusPanel.Show("You must activate a character this turn.");
				return;
			}

			lockedInCharacter?.EndTurn();
			lockedInCharacter = null;
			selectedCharacter = null;

			if (data.OtherSideHasNoRemainingCharactersAndIHaveSome)
			{
				statusPanel.Show("Opponent has no more characters to activate, go again!");
				return;
			}

			data.LastPlayingPlayerId = ApplicationData.PlayerId;
			data.ChangePlayingSide();
			data.TurnNumber++;
			if (!data.AnyCharacterUnactivated)
				StartNextRound();

			if (!data.IsTestMode)
				await battleManager.SaveBattle(data);
			else
				playerSideText.Value = $"Your side: {data.Sides[data.GetPlayerId()]}";
		}

		private async Task CheckForWin()
		{
			var spaghettiPlayerId = data.Sides.Single(pair => pair.Value == CharacterSide.Spaghetti).Key;
			var anySpaghetti = data.Characters.Any(character => character.OwnerPlayerId == spaghettiPlayerId);
			var unicornPlayerId = data.Sides.Single(pair => pair.Value == CharacterSide.Unicorn).Key;
			var anyUnicorn = data.Characters.Any(character => character.OwnerPlayerId == unicornPlayerId);

			if (!anySpaghetti || !anyUnicorn)
			{
				if (!anySpaghetti && anyUnicorn)
					data.SetWinnerSide(CharacterSide.Unicorn);
				else if (anySpaghetti && !anyUnicorn)
					data.SetWinnerSide(CharacterSide.Spaghetti);

				await battleManager.SaveBattle(data);

				GameOver?.Invoke(data);
			}
		}

		public async void Update(GameTime gameTime)
		{
			mapRenderer.Update(gameTime);

			while (TouchPanel.IsGestureAvailable)
			{
				var gesture = TouchPanel.ReadGesture();
				screenComponent.InvokeContainerGestureRead(gesture);

				if (gesture.GestureType == GestureType.FreeDrag)
					Pan(gesture);
				if (gesture.GestureType == GestureType.Pinch)
					Zoom(gesture);
				if (gesture.GestureType == GestureType.Tap)
				{
					screenComponent.InvokeContainerTap(gesture);
					await Tap(gesture);
				}
				if (gesture.GestureType == GestureType.Hold)
					screenComponent.InvokeContainerHeld(gesture);
			}

			screenComponent.Update(gameTime);

			newTurnButton.IsHighlighted = lockedInCharacter != null && !lockedInCharacter.HasRemainingMoves && !lockedInCharacter.HasRemainingPlayableCards;
		}

		private bool IsMyTurn
		{
			get { return data.CurrentPlayerId == data.GetPlayerId(); }
		}

		public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Rectangle clientBounds)
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

			if (IsMyTurn)
			{
				spriteBatch.Begin(
					transformMatrix: camera.GetViewMatrix(),
					samplerState: SamplerState.PointClamp,
					blendState: BlendState.AlphaBlend,
					sortMode: SpriteSortMode.Texture);
				if (selectedCharacter != null && selectedCard == null && movingPoint == null)
				{
					data.Characters.SingleOrDefault(character => character == selectedCharacter && character.HasRemainingMoves && character.OwnerPlayerId == data.GetPlayerId())?.DrawMovementRange(spriteBatch);
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
			}

			Effect currentEffect = null;
			bool startedSpriteBatch = false;
			Func<IDrawable, Effect> effectForDrawable = drawable =>
			{
				var character = drawable as Character;
				if (character == null)
					return null;
				if (character == selectedCharacter)
					return content.Highlight;
				if (character.ActivatedThisRound)
					return content.Shade;
				return null;
			};

			foreach (var drawable in entities.Drawables)
			{
				var characterEffect = effectForDrawable(drawable);
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

				if (drawable == lockedInCharacter)
					lockedInCharacter.DrawLock(spriteBatch);

				drawable.Draw(spriteBatch);
			}
			spriteBatch.End();

			// Draw a characters hand of cards if player is playing that side
			if (selectedCharacter != null && selectedCharacter.OwnerPlayerId == data.GetPlayerId())
				handOfCards.Draw(spriteBatch, clientBounds, selectedCharacter.CurrentHand, selectedCard);

			// Draw UI
			spriteBatch.Begin();
			if (IsMyTurn)
				newTurnButton.Draw(spriteBatch, playerSidePanel.Width + 2 * UIComponent.MARGIN, UIComponent.MARGIN);
			statusPanel.Draw(spriteBatch, UIComponent.MARGIN, UIComponent.MARGIN);
			playerSidePanel.Draw(spriteBatch, UIComponent.MARGIN, UIComponent.MARGIN);
			if (data.IsTestMode)
				winGameNowButton.Draw(spriteBatch, newTurnButton.Width + playerSidePanel.Width + 3 * UIComponent.MARGIN, UIComponent.MARGIN);

			gameLogPanel.Draw(spriteBatch, UIComponent.MARGIN, newTurnButton.Height + 2 * UIComponent.MARGIN);

			if (selectedCharacter != null)
				selectedPlayerStatsPanel.Draw(spriteBatch, gameLogPanel.Width + 2 * UIComponent.MARGIN, newTurnButton.Height + 2 * UIComponent.MARGIN);

			if (showGameLogEntryPanel)
				heldGameLogEntryPanel.Draw(spriteBatch);

			if (showAppliedConditionDetailsPanel)
				appliedConditionDetailsPanel.Draw(spriteBatch);

			spriteBatch.End();

			graphicsDevice.SetRenderTarget(null);
			spriteBatch.Begin();
			spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
			spriteBatch.End();
		}

		public void screenComponent_GestureRead(GestureSample gesture)
		{
			showGameLogEntryPanel = false;
			showAppliedConditionDetailsPanel = false;
		}

		public async void newTurnButton_Tapped(string id, GestureSample gesture)
		{
			await NextPlayerTurn();
		}

		public void winGameNowButton_Tapped(string id, GestureSample gesture)
		{
			data.SetWinnerSide(data.Sides[data.GetPlayerId()]);
			GameOver?.Invoke(data);
		}

		public void gameLogEntryRow_Held(UIComponent component, GestureSample gesture)
		{
			var heldLogEntryRow = component as GameLogEntryRow;
			if (heldLogEntryRow == null)
				return;

			showGameLogEntryPanel = true;
			heldGameLogEntryPanel.Position = gesture.Position;

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

		public void appliedConditionRow_Held(UIComponent component, GestureSample gesture)
		{
			showAppliedConditionDetailsPanel = true;
			appliedConditionDetailsPanel.Position = gesture.Position;
			appliedConditionDetailsText.Value = selectedCharacter.Conditions.Aggregate("", (total, condition) => condition.Condition.Text + "\n" + total).Trim();
		}

		private async Task Tap(GestureSample gesture)
		{
			// Can't tap if it isn't your turn.
			//          if (data.CurrentSidesTurn != thisPlayerSide)
			//return;

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
			var targetEntity = entities.Drawables.SingleOrDefault(entity => entity.IsAtPoint(mapPoint));
			if (selectedCharacter == null)
			{
				// SELECT CHARACTER
				// we don't have a character selected, so select whatever we have under the tap
				selectedCharacter = targetCharacter;
				SelectedCharacterChange?.Invoke(selectedCharacter);
			}
			else if (selectedCard != null && targetCharacter != null && selectedCard.IsWithinRangeDistance(mapPoint) && mapPoint.IsWithinMap && (lockedInCharacter == selectedCard.OwnerCharacter || lockedInCharacter == null) && targetingPoint == null && IsMyTurn)
			{
				// TRYING TO TARGET A CARD
				targetingPoint = mapPoint;
			}
			else if (targetingPoint?.Equals(mapPoint) == true && IsMyTurn)
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
							await CheckForWin();
						}

						AddNewGameLogEntry(new GameLogEntry
						{
							SourceCharacterColour = selectedCharacter.Colour,
							SourceCharacterSide = data.Sides[selectedCharacter.OwnerPlayerId],
							TargetCharacterColour = targetCharacter.Colour,
							TargetCharacterSide = data.Sides[targetCharacter.OwnerPlayerId],
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
			else if (selectedCharacter.IsWithinMoveDistanceOf(mapPoint) && mapPoint.IsWithinMap && mapPoint.IsInAvailableMovement && CanMoveSelectedCharacter && targetEntity == null && movingPoint == null)
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
					SourceCharacterColour = selectedCharacter.Colour,
					SourceCharacterSide = data.Sides[selectedCharacter.OwnerPlayerId],
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

		private bool CanMoveSelectedCharacter => selectedCharacter.OwnerPlayerId == data.GetPlayerId()
					&& selectedCharacter.HasRemainingMoves
					&& !selectedCharacter.ActivatedThisRound
					&& (lockedInCharacter == selectedCharacter || lockedInCharacter == null)
					&& IsMyTurn;

		private bool CanSelectCardFromSelectedCharacter => selectedCharacter != null && selectedCharacter.HasRemainingPlayableCards && !selectedCharacter.ActivatedThisRound && IsMyTurn;

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
				var a = gesture.Position;
				var b = gesture.Position2;
				var dist = Vector2.Distance(a, b);

				var aOld = gesture.Position - gesture.Delta;
				var bOld = gesture.Position2 - gesture.Delta2;
				var distOld = Vector2.Distance(aOld, bOld);

				var scale = (distOld - dist) * -0.05f;
				if (camera.Zoom + scale > 5 || camera.Zoom + scale < 1)
					return;

				camera.Zoom += scale;
			}
		}
	}
}
