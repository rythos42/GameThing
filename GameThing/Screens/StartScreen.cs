using GameThing.Entities;
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
		private readonly Button startAsSpaghetti = new Button("Start as Spaghetti");
		private readonly Button signIn = new Button("Sign In");
		private readonly Button createMatch = new Button("Create Match");
		private readonly Button joinMatch = new Button("Join Match");

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

		public void LoadContent(Content content, ContentManager contentManager, GraphicsDevice graphicsDevice)
		{
			startAsSpaghetti.LoadContent(content, contentManager, graphicsDevice);
			signIn.LoadContent(content, contentManager, graphicsDevice);
			createMatch.LoadContent(content, contentManager, graphicsDevice);
			joinMatch.LoadContent(content, contentManager, graphicsDevice);
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

			startAsSpaghetti.Draw(spriteBatch, 40, 40);

			if (!appData.SignedIn)
			{
				signIn.Draw(spriteBatch, 40, 160);
			}
			else
			{
				createMatch.Draw(spriteBatch, 40, 160);
				joinMatch.Draw(spriteBatch, 40, 280);
			}

			spriteBatch.End();
		}
	}
}
