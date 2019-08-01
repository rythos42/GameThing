using GameThing.Events;
using GameThing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameThing.Screens
{
	public class StartScreen
	{
		private readonly Button startAsSpaghetti = new Button("Start as Spaghetti", 40, 40, 300, 75);
		private readonly Button signIn = new Button("Sign In", 40, 160, 300, 75);
		private readonly Button createMatch = new Button("Create Match", 40, 160, 300, 75);
		private readonly Button joinMatch = new Button("Join Match", 40, 280, 300, 75);

		private ApplicationData appData;

		public event MatchEventHandler CreateMatch;
		public event MatchEventHandler JoinMatch;
		public event StartGameEventHandler Started;
		public event RequestSignInEventHandler RequestSignIn;
		public delegate void StartGameEventHandler(CharacterSide side);

		public StartScreen(ApplicationData appData)
		{
			this.appData = appData;
		}

		public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
		{
			startAsSpaghetti.LoadContent(content, graphicsDevice);
			signIn.LoadContent(content, graphicsDevice);
			createMatch.LoadContent(content, graphicsDevice);
			joinMatch.LoadContent(content, graphicsDevice);
		}

		public void Update(GameTime gameTime)
		{
			var gesture = default(GestureSample);

			while (TouchPanel.IsGestureAvailable)
			{
				gesture = TouchPanel.ReadGesture();

				if (gesture.GestureType == GestureType.Tap)
					Tap(gesture);
			}
		}

		private void Tap(GestureSample gesture)
		{
			if (startAsSpaghetti.IsVisible && startAsSpaghetti.IsAtPoint(gesture.Position))
				Started?.Invoke(CharacterSide.Spaghetti);

			if (createMatch.IsVisible && createMatch.IsAtPoint(gesture.Position))
				CreateMatch?.Invoke();

			if (joinMatch.IsVisible && joinMatch.IsAtPoint(gesture.Position))
				JoinMatch?.Invoke();

			if (signIn.IsVisible && signIn.IsAtPoint(gesture.Position))
				RequestSignIn?.Invoke();
		}

		public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			graphicsDevice.Clear(Color.White);

			spriteBatch.Begin();

			startAsSpaghetti.Draw(spriteBatch);

			if (!appData.SignedIn)
			{
				signIn.Draw(spriteBatch);
			}
			else
			{
				createMatch.Draw(spriteBatch);
				joinMatch.Draw(spriteBatch);
			}

			spriteBatch.End();
		}
	}
}
