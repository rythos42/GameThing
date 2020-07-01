using GameThing.Contract;
using GameThing.Database;
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

		private readonly BattleScreen battleScreen = new BattleScreen();
		private readonly StartScreen startScreen = new StartScreen();
		private readonly GameOverScreen gameOverScreen = new GameOverScreen();
		private readonly ManageTeamScreen manageTeamScreen = new ManageTeamScreen();
		private readonly ManageCharacterScreen manageCharacterScreen = new ManageCharacterScreen();

		private event ContentLoadedEventHandler ContentLoaded;

		public MainGame()
		{
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
			manageTeamScreen.CharacterTapped += ManageTeamScreen_CharacterTapped;

			CategoryMapper.Instance.Load();
			CardMapper.Instance.Load();
			CharacterClassMapper.Instance.Load();
		}

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

		private void ManageTeamScreen_CharacterTapped(Character character)
		{
			manageCharacterScreen.Character = character;
			ApplicationData.CurrentScreen = ScreenType.ManageCharacter;
		}

		protected override void Initialize()
		{
			TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.Tap | GestureType.Hold;

			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			var content = new Content(Content, GraphicsDevice);

			battleScreen.LoadContent(content, GraphicsDevice);
			startScreen.LoadContent(content, GraphicsDevice);
			gameOverScreen.LoadContent(content, GraphicsDevice);
			manageTeamScreen.LoadContent(content, GraphicsDevice);
			manageCharacterScreen.LoadContent(content, GraphicsDevice);

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
						ApplicationData.CurrentScreen = ScreenType.Start; break;
					case ScreenType.ManageCharacter:
						ApplicationData.CurrentScreen = ScreenType.ManageTeams; break;
					case ScreenType.Start:
						Exit();
						break;
				}
			}

			switch (ApplicationData.CurrentScreen)
			{
				case ScreenType.Battle: battleScreen.Update(gameTime); break;
				case ScreenType.Start: startScreen.Update(gameTime); break;
				case ScreenType.ManageTeams: manageTeamScreen.Update(gameTime); break;
				case ScreenType.ManageCharacter: manageCharacterScreen.Update(gameTime); break;
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			switch (ApplicationData.CurrentScreen)
			{
				case ScreenType.Battle: battleScreen.Draw(GraphicsDevice, spriteBatch, Window.ClientBounds); break;
				case ScreenType.Start: startScreen.Draw(GraphicsDevice, spriteBatch); break;
				case ScreenType.GameOver: gameOverScreen.Draw(GraphicsDevice, spriteBatch); break;
				case ScreenType.ManageTeams: manageTeamScreen.Draw(GraphicsDevice, spriteBatch); break;
				case ScreenType.ManageCharacter: manageCharacterScreen.Draw(GraphicsDevice, spriteBatch); break;
			}

			base.Draw(gameTime);
		}
	}
}
