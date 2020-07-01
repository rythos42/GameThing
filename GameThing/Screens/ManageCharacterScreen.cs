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
		private readonly Panel categoryPanel = new Panel { X = 320, Y = 320 };
		private readonly Panel contentPanel = new Panel { ShowBorder = true, X = 400, Y = 300 };

		private Character character;

		public ManageCharacterScreen()
		{
			backButton = new Button("Back") { Tapped = BackButton_Tapped, X = 200, Y = 100 };
		}

		public Character Character
		{
			get => character;
			set
			{
				character = value;

				categoryPanel.Components.Clear();
				foreach (var levelKeyPair in character.CategoryLevels)
				{
					if (levelKeyPair.Value == 0)
						continue;

					var text = $"{levelKeyPair.Key}: {levelKeyPair.Value}";
					categoryPanel.Components.Add(new Text { Value = text });
				}
			}
		}

		public void BackButton_Tapped(string id, GestureSample gesture)
		{
			ApplicationData.CurrentScreen = ScreenType.ManageTeams;
		}

		public void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			screenComponent = new ScreenComponent() { Background = content.MainBackground };
			screenComponent.Components.Add(backButton);
			screenComponent.Components.Add(contentPanel);
			//screenComponent.Components.Add(categoryPanel);
			screenComponent.LoadContent(content, graphicsDevice);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Part of public API")]
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
			backButton.Draw(spriteBatch);
			//categoryPanel.Draw(spriteBatch);
			contentPanel.Draw(spriteBatch);

			spriteBatch.End();
		}
	}
}
