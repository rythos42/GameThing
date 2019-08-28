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
		private SpriteFont font;

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

			font = content.Font;
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

			spriteBatch.DrawString(font, "Welcome to the MVP of GameThing!", new Vector2(500, 40), Color.Black);
			spriteBatch.DrawString(font, "You play as a team of 5 characters.", new Vector2(500, 90), Color.Black);
			spriteBatch.DrawString(font, "Each character has a deck of 8 cards, with 4 cards in hand at a time.", new Vector2(500, 120), Color.Black);
			spriteBatch.DrawString(font, "Characters can play 2 cards and move 5 in their turn.", new Vector2(500, 150), Color.Black);
			spriteBatch.DrawString(font, "Once you play a card or move a character, you can't choose another this turn.", new Vector2(500, 180), Color.Black);
			spriteBatch.DrawString(font, "When you choose to be finished for a turn, press New Turn and your opponent will play.", new Vector2(500, 210), Color.Black);
			spriteBatch.DrawString(font, "After one side looses all characters, the other side wins!", new Vector2(500, 240), Color.Black);

			spriteBatch.DrawString(font, "The MVP only has play versus another person and is turn-based.", new Vector2(500, 290), Color.Black);

			spriteBatch.End();
		}
	}
}
