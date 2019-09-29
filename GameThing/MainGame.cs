﻿using System.Collections.Generic;
using GameThing.Data;
using GameThing.Entities;
using GameThing.Events;
using GameThing.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameThing
{
	public class MainGame : Game
	{
		private readonly GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;

		private ScreenType currentScreen = ScreenType.StartMenu;
		private readonly BattleScreen battleScreen;
		private readonly StartScreen startScreen;
		private readonly GameOverScreen gameOverScreen = new GameOverScreen();

		public event MatchEventHandler CreateMatch;
		public event MatchEventHandler JoinMatch;
		public event NextPlayersTurnEventHandler NextPlayersTurn;
		public event RequestSignInEventHandler RequestSignIn;
		public event GameOverEventHandler GameOver;
		private event BattleContentLoadedEventHandler BattleContentLoaded;

		private readonly ApplicationData appData = new ApplicationData();
		private Content content;
		private readonly CardManager cardManager = new CardManager();

		public MainGame()
		{
			startScreen = new StartScreen(appData);
			battleScreen = new BattleScreen(cardManager);

			graphics = new GraphicsDeviceManager(this)
			{
				IsFullScreen = true,
				PreferredBackBufferHeight = Window.ClientBounds.Height,
				PreferredBackBufferWidth = Window.ClientBounds.Width,
				SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight
			};
			Content.RootDirectory = "Content";

			startScreen.StartAsTester += StartScreen_StartedAsTester;
			startScreen.CreateMatch += StartScreen_CreateMatch;
			startScreen.JoinMatch += StartScreen_JoinMatch;
			startScreen.RequestSignIn += StartScreen_RequestSignIn;
			battleScreen.GameOver += BattleScreen_GameOver;
			battleScreen.NextPlayersTurn += BattleScreen_NextPlayersTurn;
		}

		public void SetSignedIn(bool signedIn)
		{
			appData.SignedIn = true;
		}

		public BattleData InitializeGameData(string matchId, IList<string> participantIds)
		{
			var battleData = new BattleData
			{
				CurrentSidesTurn = CharacterSide.Spaghetti,
				MatchId = matchId
			};

			battleData.Characters.Add(CreateCharacter(CharacterSide.Spaghetti, CharacterClass.Apprentice, CharacterColour.Blue));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Spaghetti, CharacterClass.Apprentice, CharacterColour.Green));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Spaghetti, CharacterClass.Pickpocket, CharacterColour.None));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Spaghetti, CharacterClass.Squire, CharacterColour.Red));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Spaghetti, CharacterClass.Squire, CharacterColour.White));

			battleData.Characters.Add(CreateCharacter(CharacterSide.Unicorn, CharacterClass.Apprentice, CharacterColour.Blue));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Unicorn, CharacterClass.Apprentice, CharacterColour.Green));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Unicorn, CharacterClass.Pickpocket, CharacterColour.None));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Unicorn, CharacterClass.Squire, CharacterColour.Red));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Unicorn, CharacterClass.Squire, CharacterColour.White));

			battleData.Characters.ForEach(character => character.InitializeDeck(cardManager));

			for (int i = 0; i < participantIds.Count; i++)
			{
				battleData.Sides[participantIds[i]] = (CharacterSide) i;
			}

			return battleData;
		}

		private Character CreateCharacter(CharacterSide side, CharacterClass cClass, CharacterColour colour)
		{
			var character = new Character(side, colour, cClass);
			character.ResetTurn();
			return character;
		}

		private void StartScreen_CreateMatch()
		{
			CreateMatch?.Invoke();
		}

		private void StartScreen_JoinMatch()
		{
			JoinMatch?.Invoke();
		}

		private void StartScreen_StartedAsTester()
		{
			var battleData = InitializeGameData(null, new List<string>());

			var side = CharacterSide.Spaghetti;
			currentScreen = ScreenType.Battle;
			battleData.CurrentSidesTurn = side;
			battleData.Sides["p1"] = CharacterSide.Spaghetti;
			battleData.Sides["p2"] = CharacterSide.Unicorn;
			battleScreen.IsTestMode = true;
			battleScreen.SetBattleData(battleData);
			battleScreen.StartGame(side);
		}

		private void StartScreen_RequestSignIn()
		{
			RequestSignIn?.Invoke();
		}

		private void BattleScreen_GameOver(BattleData data)
		{
			ShowGameOver(data);
		}

		private void BattleScreen_NextPlayersTurn(BattleData data)
		{
			NextPlayersTurn?.Invoke(data);
		}

		public void StartMatch(string myParticipantId, BattleData gameData)
		{
			BattleContentLoadedEventHandler startMatch = null;
			startMatch = () =>
			{
				currentScreen = ScreenType.Battle;
				battleScreen.SetBattleData(gameData);
				battleScreen.StartGame(myParticipantId);

				// remove self from event
				BattleContentLoaded -= startMatch;
			};

			// If we haven't loaded content yet, set an event to start the match after we do so.
			if (content == null)
				BattleContentLoaded += startMatch;
			else
				startMatch();
		}

		public void ShowGameOver(BattleData data)
		{
			GameOver?.Invoke(data);

			gameOverScreen.Winner = data.WinnerSide;
			currentScreen = ScreenType.GameOver;
		}

		protected override void Initialize()
		{
			TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.Tap | GestureType.Hold;

			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			content = new Content(Content);

			battleScreen.LoadContent(content, GraphicsDevice);
			BattleContentLoaded?.Invoke();

			startScreen.LoadContent(content, GraphicsDevice);
			gameOverScreen.LoadContent(content, GraphicsDevice);
		}

		protected override void UnloadContent()
		{
			Content.Unload();
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
			{
				switch (currentScreen)
				{
					case ScreenType.GameOver:
					case ScreenType.Battle:
						currentScreen = ScreenType.StartMenu; break;
					case ScreenType.StartMenu:
						Exit();
						break;
				}
			}

			switch (currentScreen)
			{
				case ScreenType.Battle:
					battleScreen.Update(gameTime);
					break;
				case ScreenType.StartMenu:
					startScreen.Update(gameTime);
					break;
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			switch (currentScreen)
			{
				case ScreenType.Battle:
					battleScreen.Draw(GraphicsDevice, spriteBatch, Window.ClientBounds, gameTime);
					break;
				case ScreenType.StartMenu:
					startScreen.Draw(GraphicsDevice, spriteBatch);
					break;
				case ScreenType.GameOver:
					gameOverScreen.Draw(GraphicsDevice, spriteBatch);
					break;
			}

			base.Draw(gameTime);
		}
	}
}
