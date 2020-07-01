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
		private readonly Panel teamPanel = new Panel { X = 1100, Y = 168 };
		private Texture2D backgroundLine;

		private TeamData team;

		private readonly TeamManager teamManager = TeamManager.Instance;

		public event CharacterTappedEventHandler CharacterTapped;

		public ManageTeamScreen()
		{
			createTeamButton = new Button("Create Team") { Tapped = CreateTeamButton_Tapped, X = 450, Y = 200 };
			deleteTeamButton = new Button("Delete Team") { Tapped = DeleteTeamButton_Tapped, X = 450, Y = 200 };
			backButton = new Button("Back") { Tapped = BackButton_Tapped, X = 450, Y = 320 };

			if (teamManager.Team == null)
				teamManager.OnTeamLoad += SetTeam;
			else
				SetTeam(team);
		}

		public void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
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
			var hasTeam = team != null;
			deleteTeamButton.IsVisible = hasTeam;
			createTeamButton.IsVisible = !hasTeam;
			teamPanel.IsVisible = hasTeam;
			if (team == null)
				return;

			for (var i = 0; i < characterCount; i++)
			{
				var character = team.Characters[i];
				teamPanel.Components.Add(new Button(character.Name) { Tapped = CharacterButton_Tapped, Id = character.Id.ToString() });
			}
		}

		public void CharacterButton_Tapped(string id, GestureSample gesture)
		{
			var selectedCharacter = teamManager.Team.Characters.Single(character => character.Id == Guid.Parse(id));
			CharacterTapped?.Invoke(selectedCharacter);
		}

		public async void CreateTeamButton_Tapped(string id, GestureSample gesture)
		{
			team = TeamData.CreateDefaultTeam();
			await teamManager.CreateTeam(team);
		}

		public async void DeleteTeamButton_Tapped(string id, GestureSample gesture)
		{
			await teamManager.DeleteTeam();
		}

		public void BackButton_Tapped(string id, GestureSample gesture)
		{
			ApplicationData.CurrentScreen = ScreenType.Start;
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

			deleteTeamButton.Draw(spriteBatch);
			createTeamButton.Draw(spriteBatch);
			backButton.Draw(spriteBatch);

			spriteBatch.Draw(backgroundLine, new Rectangle((graphicsDevice.Viewport.Width / 2) - 15, (int) (graphicsDevice.Viewport.Height * 0.125), 30, (int) (graphicsDevice.Viewport.Height * 0.75)), Color.White);

			teamPanel.Draw(spriteBatch);

			spriteBatch.End();
		}
	}
}
