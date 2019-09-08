using GameThing.Data;
using GameThing.Entities;
using GameThing.Events;
using GameThing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameThing.Screens
{
	public class StartScreen : UIEventContainer
	{
		private readonly Button startAsTester;
		private readonly Button signIn;
		private readonly Button createMatch;
		private readonly Button joinMatch;
		private SpriteFont font;

		private readonly ApplicationData appData;

		public event MatchEventHandler CreateMatch;
		public event MatchEventHandler JoinMatch;
		public event StartGameAsTesterEventHandler StartAsTester;
		public event RequestSignInEventHandler RequestSignIn;

		public StartScreen(ApplicationData appData)
		{
			this.appData = appData;

			startAsTester = new Button("Test") { Tapped = startAsTester_Tapped };
			Components.Add(startAsTester);

			signIn = new Button("Sign In") { Tapped = signIn_Tapped };
			Components.Add(signIn);

			createMatch = new Button("Create Match") { Tapped = createMatch_Tapped };
			Components.Add(createMatch);

			joinMatch = new Button("Join Match") { Tapped = joinMatch_Tapped };
			Components.Add(joinMatch);
		}

		public void startAsTester_Tapped(GestureSample gesture)
		{
			StartAsTester?.Invoke();
		}

		public void signIn_Tapped(GestureSample gesture)
		{
			RequestSignIn?.Invoke();
		}

		public void createMatch_Tapped(GestureSample gesture)
		{
			CreateMatch?.Invoke();
		}

		public void joinMatch_Tapped(GestureSample gesture)
		{
			JoinMatch?.Invoke();
		}

		public override void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			base.LoadContent(content, graphicsDevice);
			font = content.Font;
		}

		public void Update(GameTime gameTime)
		{
			var gesture = default(GestureSample);

			while (TouchPanel.IsGestureAvailable)
			{
				gesture = TouchPanel.ReadGesture();

				if (gesture.GestureType == GestureType.Tap)
					InvokeContainerTap(gesture);
			}
		}

		public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			graphicsDevice.Clear(Color.White);

			spriteBatch.Begin();

			startAsTester.Draw(spriteBatch, 40, 40);

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
