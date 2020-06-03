using GameThing.Contract;
using GameThing.Entities;
using GameThing.Manager;
using GameThing.Screens;
using GameThing.UI;
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

		private readonly BattleScreen battleScreen;
		private readonly StartScreen startScreen = new StartScreen();
		private readonly GameOverScreen gameOverScreen = new GameOverScreen();
		private readonly ManageTeamScreen manageTeamScreen;

		private event ContentLoadedEventHandler ContentLoaded;

		public MainGame()
		{
			battleScreen = new BattleScreen(CardManager);
			manageTeamScreen = new ManageTeamScreen(CardManager);

			graphics = new GraphicsDeviceManager(this)
			{
				IsFullScreen = true,
				PreferredBackBufferHeight = Window.ClientBounds.Height,
				PreferredBackBufferWidth = Window.ClientBounds.Width,
				SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight
			};
			Content.RootDirectory = "Content";

			battleScreen.GameOver += BattleScreen_GameOver;
			startScreen.StartBattle += StartScreen_StartBattle;
		}

		public CardManager CardManager { get; private set; } = new CardManager();

		private async void BattleScreen_GameOver(BattleData battleData)
		{
			await TeamManager.Instance.MergeBattleDataAndSaveTeam(battleData);
			gameOverScreen.Winner = battleData.WinnerSide;
			ApplicationData.CurrentScreen = ScreenType.GameOver;
		}

		private void StartScreen_StartBattle(BattleData data)
		{
			battleScreen.StartGame(data);
			ApplicationData.CurrentScreen = ScreenType.Battle;
		}

		protected override void Initialize()
		{
			TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.Tap | GestureType.Hold;

			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			var content = new Content(Content);

			battleScreen.LoadContent(content, GraphicsDevice);
			startScreen.LoadContent(content, GraphicsDevice);
			gameOverScreen.LoadContent(content, GraphicsDevice);
			manageTeamScreen.LoadContent(content, GraphicsDevice);

			ContentLoaded?.Invoke();
		}

		protected override void UnloadContent()
		{
			Content.Unload();

			BattleManager.Instance.Dispose();
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
			{
				switch (ApplicationData.CurrentScreen)
				{
					case ScreenType.GameOver:
					case ScreenType.Battle:
					case ScreenType.ManageTeams:
						ApplicationData.CurrentScreen = ScreenType.StartMenu; break;
					case ScreenType.StartMenu:
						Exit();
						break;
				}
			}

			switch (ApplicationData.CurrentScreen)
			{
				case ScreenType.Battle:
					battleScreen.Update(gameTime);
					break;
				case ScreenType.StartMenu:
					startScreen.Update(gameTime);
					break;
				case ScreenType.ManageTeams:
					manageTeamScreen.Update(gameTime);
					break;
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			switch (ApplicationData.CurrentScreen)
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
				case ScreenType.ManageTeams:
					manageTeamScreen.Draw(GraphicsDevice, spriteBatch);
					break;
			}

			base.Draw(gameTime);
		}
	}
}
