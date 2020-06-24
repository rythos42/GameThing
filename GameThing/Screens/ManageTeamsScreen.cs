using System;
using System.Linq;
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
		private const int characterCount = 5;

		private ScreenComponent screenComponent;
		private readonly Button createTeamButton;
		private readonly Button deleteTeamButton;
		private readonly Button backButton;
		private readonly Panel teamPanel = new Panel { ShowChrome = false };
		private Texture2D backgroundLine;

		private TeamData team;

		private readonly TeamManager teamManager = TeamManager.Instance;

		public event CharacterTappedEventHandler CharacterTapped;

		public ManageTeamScreen()
		{
			createTeamButton = new Button("Create Team") { Tapped = createTeamButton_Tapped };
			deleteTeamButton = new Button("Delete Team") { Tapped = deleteTeamButton_Tapped };
			backButton = new Button("Back") { Tapped = backButton_Tapped };

			if (teamManager.Team == null)
				teamManager.OnTeamLoad += SetTeam;
			else
				SetTeam(team);
		}

		public void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			if (teamPanel.Components.Count == 0)
			{
				for (int i = 0; i < characterCount; i++)
				{
					var characterButton = new Button("No team loaded.") { Tapped = characterButton_Tapped, Id = null };
					teamPanel.Components.Add(characterButton);
				}
			}

			screenComponent = new ScreenComponent(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight) { Background = content.MainBackground };
			screenComponent.Components.Add(createTeamButton);
			screenComponent.Components.Add(deleteTeamButton);
			screenComponent.Components.Add(backButton);
			screenComponent.Components.Add(teamPanel);
			screenComponent.LoadContent(content, graphicsDevice);

			backgroundLine = content.BackgroundLine;
		}

		private void SetTeam(TeamData team)
		{
			for (int i = 0; i < characterCount; i++)
			{
				var character = team.Characters[i];

				if (teamPanel.Components.Count <= i)
					teamPanel.Components.Add(new Button(character.Name) { Tapped = characterButton_Tapped, Id = character.Id.ToString() });
				else
					((Button) teamPanel.Components[i]).Text = character.Name;
			}
		}

		public void characterButton_Tapped(string id, GestureSample gesture)
		{
			var selectedCharacter = teamManager.Team.Characters.Single(character => character.Id == Guid.Parse(id));
			CharacterTapped?.Invoke(selectedCharacter);
		}

		public async void createTeamButton_Tapped(string id, GestureSample gesture)
		{
			team = TeamData.CreateDefaultTeam();
			await teamManager.CreateTeam(team);
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

			spriteBatch.Draw(backgroundLine, new Rectangle((graphicsDevice.Viewport.Width / 2) - 15, (int) (graphicsDevice.Viewport.Height * 0.125), 30, (int) (graphicsDevice.Viewport.Height * 0.75)), Color.White);

			teamPanel.DrawConditional(spriteBatch, 1100, 168, teamManager.HasTeam);

			spriteBatch.End();
		}
	}
}
