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
#pragma warning disable IDE0052 // Remove unread private members
		private readonly GraphicsDeviceManager graphics;
#pragma warning restore IDE0052 // Remove unread private members
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

		private readonly ApplicationData appData = new ApplicationData();
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

			startScreen.StartAsTester += StartScreen_StartedAsTester;
			startScreen.CreateMatch += StartScreen_CreateMatch;
			startScreen.JoinMatch += StartScreen_JoinMatch;
			startScreen.RequestSignIn += StartScreen_RequestSignIn;
			battleScreen.GameOver += BattleScreen_GameOver;
			battleScreen.NextPlayersTurn += BattleScreen_NextPlayersTurn;
		}

		public TeamData TeamData { get; set; }

		public void SetSignedIn(bool signedIn)
		{
			appData.SignedIn = signedIn;
		}

		public BattleData InitializeBattleData(string matchId)
		{
			return new BattleData
			{
				CurrentSidesTurn = CharacterSide.Spaghetti,
				MatchId = matchId
			};
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
			var battleData = InitializeBattleData("test");
			battleData.InitializeCharacters("participantId", TeamData);

			currentScreen = ScreenType.Battle;
			battleScreen.IsTestMode = true;
			battleScreen.SetBattleData(battleData);
			battleScreen.StartGame(CharacterSide.Spaghetti);
		}

		private void StartScreen_RequestSignIn()
		{
			RequestSignIn?.Invoke();
		}

		private void BattleScreen_GameOver(BattleData data)
		{
			TeamData.MergeBattleData(data);

			GameOver?.Invoke(data.MatchId, TeamData);
			ShowGameOver(data);
		}

		private void BattleScreen_NextPlayersTurn(BattleData data)
		{
			NextPlayersTurn?.Invoke(data);
		}

		public void StartMatch(string myParticipantId, BattleData gameData)
		{
			currentScreen = ScreenType.Battle;
			battleScreen.IsTestMode = false;
			battleScreen.SetBattleData(gameData);
			battleScreen.StartGame(myParticipantId);
		}

		public void ShowGameOver(BattleData data)
		{
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

			content = new Content(Content);

			battleScreen.LoadContent(content, GraphicsDevice);
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
					battleScreen.Draw(GraphicsDevice, spriteBatch, Window.ClientBounds);
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
