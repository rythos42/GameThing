using GameThing.Entities;
using GameThing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameThing.Screens
{
	public class ManageCharacterScreen
	{
		private ScreenComponent screenComponent;
		private readonly Button backButton;

		public ManageCharacterScreen()
		{
			backButton = new Button("Back") { Tapped = backButton_Tapped };
		}

		public Character Character { get; set; }

		public void backButton_Tapped(string id, GestureSample gesture)
		{
			ApplicationData.CurrentScreen = ScreenType.ManageTeams;
		}

		public void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			screenComponent = new ScreenComponent(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight) { Background = content.MainBackground };
			screenComponent.Components.Add(backButton);
			screenComponent.LoadContent(content, graphicsDevice);
		}

		public void Update(GameTime gameTime)
		{
			while (TouchPanel.IsGestureAvailable)
			{
				var gesture = TouchPanel.ReadGesture();

				if (gesture.GestureType == GestureType.Tap)
					screenComponent.InvokeContainerTap(gesture);
			}
		}

		public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			graphicsDevice.Clear(Color.White);

			spriteBatch.Begin();
			screenComponent.Draw(spriteBatch);
			backButton.Draw(spriteBatch, 450, 320);

			spriteBatch.End();
		}
	}
}
