using System.Linq;
using GameThing.Entities;
using GameThing.Entities.Cards;
using GameThing.Entities.Content;
using GameThing.Events;
using GameThing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;

namespace GameThing.Screens
{
	public class BattleScreen
	{
		private TiledMap map;
		private TiledMapRenderer mapRenderer;
		private Camera2D camera;

		private BattleData data;
		private Character selectedCharacter;
		private HandOfCards handOfCards = new HandOfCards();
		private Card selectedCard;
		private CharacterSide thisPlayerSide;
		private Character lockedInCharacter = null;

		private Button newTurnButton = new Button("New Turn", 40, 40, 300, 75);
		private FadingTextPanel statusPanel = new FadingTextPanel(40, 40) { PlaceFromRight = true };

		private CharacterContent characterContent;
		private CardContent cardContent;

		public event NextPlayersTurnEventHandler NextPlayersTurn;
		public event GameOverEventHandler GameOver;

		public void SetBattleData(BattleData gameData)
		{
			gameData.Characters.ForEach(character =>
			{
				character.SetContent(characterContent);
				character.Deck.ForEach(card => card.SetContent(cardContent));
			});
			data = gameData;

			// Start next round if all characters activated
			var anyNotActivated = data.Characters.Any(character => !character.ActivatedThisRound);
			if (!anyNotActivated)
				StartNextRound();

			// Show NEW ROUND for first two players, have to recalculate after StartNextRound or it won't show NEW ROUND
			var countOfActivatedPlayers = data.Characters.Count(character => character.ActivatedThisRound);
			if (countOfActivatedPlayers <= 2)
				statusPanel.Show("NEW ROUND");
		}

		public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
		{
			map = content.Load<TiledMap>("tilemaps/Map");
			mapRenderer = new TiledMapRenderer(graphicsDevice);
			camera = new Camera2D(graphicsDevice);
			camera.LookAt(new Vector2(0, map.HeightInPixels / 2));  // center the map in the screen
			newTurnButton.LoadContent(content, graphicsDevice);

			statusPanel.LoadContent(content, graphicsDevice);

			var commonContent = new CommonContent(content);
			characterContent = new CharacterContent(content, commonContent);
			cardContent = new CardContent(content, commonContent);
		}

		public void StartGame(string myParticipantId)
		{
			thisPlayerSide = data.Sides[myParticipantId];
		}

		public void StartGame(CharacterSide side)
		{
			thisPlayerSide = side;
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
			mapRenderer.Update(map, gameTime);

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
		}

		public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Rectangle clientBounds, GameTime gameTime)
		{
			graphicsDevice.Clear(Color.CornflowerBlue);

			// Follow camera
			spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
			mapRenderer.Draw(map, camera.GetViewMatrix());
			data.Characters.Sort(new CharacterDepthComparer());
			data.Characters.SingleOrDefault(character => character == selectedCharacter && character.HasRemainingMoves && character.Side == thisPlayerSide)?.DrawMovementRange(spriteBatch);
			selectedCard?.DrawEffectRange(spriteBatch);
			data.Characters.ForEach(character => character.Draw(spriteBatch));
			spriteBatch.End();

			// Don't follow camera
			spriteBatch.Begin();
			// Draw a characters hand of cards if player is playing that side
			if (selectedCharacter != null && selectedCharacter.Side == thisPlayerSide && thisPlayerSide == data.CurrentSidesTurn)
				handOfCards.Draw(spriteBatch, clientBounds, selectedCharacter.CurrentHand);

			if (thisPlayerSide == data.CurrentSidesTurn)
				newTurnButton.Draw(spriteBatch);

			statusPanel.Draw(spriteBatch);
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
			else if (selectedCharacter.IsWithinMoveDistanceOf(mapPoint) && CanMoveSelectedCharacter && targetCharacter == null)
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
