using System;
using System.Collections.Generic;
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
		private readonly BattleScreen battleScreen = new BattleScreen();
		private readonly StartScreen startScreen;
		private readonly GameOverScreen gameOverScreen = new GameOverScreen();

		public event MatchEventHandler CreateMatch;
		public event MatchEventHandler JoinMatch;
		public event NextPlayersTurnEventHandler NextPlayersTurn;
		public event RequestSignInEventHandler RequestSignIn;
		public event GameOverEventHandler GameOver;

		private ApplicationData appData = new ApplicationData();
		private Content content;

		public MainGame()
		{
			startScreen = new StartScreen(appData);

			graphics = new GraphicsDeviceManager(this)
			{
				IsFullScreen = true,
				PreferredBackBufferHeight = Window.ClientBounds.Height,
				PreferredBackBufferWidth = Window.ClientBounds.Width,
				SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight
			};
			Content.RootDirectory = "Content";

			startScreen.Started += StartScreen_StartedAsTester;
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

			battleData.Characters.Add(CreateCharacter(CharacterSide.Spaghetti, CharacterColour.Blue));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Spaghetti, CharacterColour.Green));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Spaghetti, CharacterColour.None));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Spaghetti, CharacterColour.Red));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Spaghetti, CharacterColour.White));

			battleData.Characters.Add(CreateCharacter(CharacterSide.Unicorn, CharacterColour.Blue));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Unicorn, CharacterColour.Green));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Unicorn, CharacterColour.None));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Unicorn, CharacterColour.Red));
			battleData.Characters.Add(CreateCharacter(CharacterSide.Unicorn, CharacterColour.White));

			battleData.Characters.ForEach(character => character.InitializeDeck());

			for (int i = 0; i < participantIds.Count; i++)
			{
				battleData.Sides[participantIds[i]] = (CharacterSide) i;
			}

			return battleData;
		}

		private Character CreateCharacter(CharacterSide side, CharacterColour colour)
		{
			var classEnumValues = Enum.GetValues(typeof(CharacterClass));
			var thisCharacterClass = new Random().Next(0, classEnumValues.Length);

			var character = new Character(side, colour, (CharacterClass) thisCharacterClass);
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

		private void StartScreen_StartedAsTester(CharacterSide side)
		{
			var battleData = InitializeGameData("test", new List<string>());

			currentScreen = ScreenType.Battle;
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
			currentScreen = ScreenType.Battle;
			battleScreen.SetBattleData(gameData);
			battleScreen.StartGame(myParticipantId);
		}

		public void ShowGameOver(BattleData data)
		{
			GameOver?.Invoke(data);

			gameOverScreen.Winner = data.Winner;
			currentScreen = ScreenType.GameOver;
		}

		protected override void Initialize()
		{
			TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.Tap;

			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			this.content = new Content(Content);

			battleScreen.LoadContent(content, Content, GraphicsDevice);
			startScreen.LoadContent(content, Content, GraphicsDevice);
			gameOverScreen.LoadContent(content, Content, GraphicsDevice);
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
