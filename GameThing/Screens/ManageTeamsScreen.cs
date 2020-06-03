using GameThing.Contract;
using GameThing.Entities;
using GameThing.Manager;
using GameThing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameThing.Screens
{
	public class ManageTeamScreen
	{
		private ScreenComponent screenComponent;
		private readonly Button createTeamButton;
		private readonly Button deleteTeamButton;
		private readonly Button backButton;

		private readonly TeamManager teamManager = TeamManager.Instance;
		private readonly CardManager cardManager;

		public ManageTeamScreen(CardManager cardManager)
		{
			this.cardManager = cardManager;

			createTeamButton = new Button("Create Team") { Tapped = createTeamButton_Tapped };
			deleteTeamButton = new Button("Delete Team") { Tapped = deleteTeamButton_Tapped };
			backButton = new Button("Back") { Tapped = backButton_Tapped };
		}

		public void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			screenComponent = new ScreenComponent(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight) { Background = content.MainBackground };
			screenComponent.Components.Add(createTeamButton);
			screenComponent.Components.Add(deleteTeamButton);
			screenComponent.Components.Add(backButton);
			screenComponent.LoadContent(content, graphicsDevice);
		}

		public async void createTeamButton_Tapped(string id, GestureSample gesture)
		{
			var defaultTeam = TeamData.CreateDefaultTeam(cardManager);
			await teamManager.CreateTeam(defaultTeam);
		}

		public async void deleteTeamButton_Tapped(string id, GestureSample gesture)
		{
			await teamManager.DeleteTeam();
		}

		public void backButton_Tapped(string id, GestureSample gesture)
		{
			ApplicationData.CurrentScreen = ScreenType.StartMenu;
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

			deleteTeamButton.DrawConditional(spriteBatch, 450, 200, teamManager.HasTeam);
			createTeamButton.DrawConditional(spriteBatch, 450, 200, !teamManager.HasTeam);
			backButton.Draw(spriteBatch, 450, 320);

			spriteBatch.End();
		}
	}
}
