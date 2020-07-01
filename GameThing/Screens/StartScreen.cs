using System.Collections.Generic;
using System.Linq;
using GameThing.Contract;
using GameThing.Entities;
using GameThing.Events;
using GameThing.Manager;
using GameThing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameThing.Screens
{
	public class StartScreen
	{
		private readonly Button startAsTester;
		private readonly Button teams;
		private readonly Button createMatch;
		private readonly Button joinMatch;
		private readonly Button myMatches;
		private readonly Button help;
		private ScreenComponent screenComponent;
		private readonly Panel matchesPanel;
		private readonly Panel helpPanel;
		private readonly FadingTextPanel statusPanel = new FadingTextPanel { Y = UIComponent.MARGIN * 4 };
		private Texture2D backgroundLine;

		private readonly TeamManager teamManager = TeamManager.Instance;
		private readonly BattleManager battleManager = BattleManager.Instance;
		private readonly List<BattleData> availableBattles = new List<BattleData>();
		private readonly List<BattleData> myBattles = new List<BattleData>();
		private bool showingAvailableMatches = true;

		public StartBattleEventHandler StartBattle;

		public StartScreen()
		{
			startAsTester = new Button("Hot Seat") { Tapped = StartHotSeat_Tapped, X = 450, Y = 200 };
			teams = new Button("Teams") { Tapped = Teams_Tapped, X = 450, Y = 320 };
			createMatch = new Button("Create Match") { Tapped = CreateMatch_Tapped, X = 450, Y = 450 };
			joinMatch = new Button("Join Match") { Tapped = JoinMatch_Tapped, Enabled = false, X = 450, Y = 560 };
			myMatches = new Button("My Matches") { Tapped = MyMatches_Tapped, X = 450, Y = 680 };
			help = new Button("Help") { Tapped = Help_Tapped, X = 450, Y = 800 };

			matchesPanel = new Panel { X = 1100, Y = 168 };
			helpPanel = new Panel() { ExtendedPadding = true, X = 400, Y = 300, IsVisible = false };

			BattleManager.Instance.DataUpdated += BattleManager_DataUpdated;
			teamManager.OnTeamLoad += TeamManager_OnTeamLoad;
		}

		private void TeamManager_OnTeamLoad(TeamData team)
		{
			var hasTeam = team != null;
			startAsTester.IsVisible = hasTeam;
			createMatch.IsVisible = hasTeam;
			joinMatch.IsVisible = hasTeam;
			myMatches.IsVisible = hasTeam;
			matchesPanel.IsVisible = hasTeam;
		}

		private void BattleManager_DataUpdated(BattleData battleData)
		{
			if (battleData.Status == BattleStatus.Finished)
				myBattles.Remove(battleData);
		}

		private void Help_Tapped(string id, GestureSample gesture)
		{
			helpPanel.IsVisible = true;
		}

		private void JoinMatch_Tapped(string id, GestureSample gesture)
		{
			joinMatch.Enabled = false;
			myMatches.Enabled = true;
			showingAvailableMatches = true;
			SetDynamicButtons();
		}

		private void MyMatches_Tapped(string id, GestureSample gesture)
		{
			joinMatch.Enabled = true;
			myMatches.Enabled = false;
			showingAvailableMatches = false;
			SetDynamicButtons();
		}

		private void StartHotSeat_Tapped(string id, GestureSample gesture)
		{
			var battleData = new BattleData { CurrentPlayerId = BattleData.TestPlayerOneId, IsTestMode = true };

			var clonedTeamOne = Convert.Clone(teamManager.Team);
			battleData.InitializeCharacters(BattleData.TestPlayerOneId, clonedTeamOne, isTestMode: true);

			var clonedTeamTwo = Convert.Clone(teamManager.Team);
			battleData.InitializeCharacters(BattleData.TestPlayerTwoId, clonedTeamTwo, isTestMode: true);

			StartBattle?.Invoke(battleData);
		}

		private void Teams_Tapped(string id, GestureSample gesture)
		{
			ApplicationData.CurrentScreen = ScreenType.ManageTeams;
		}

		private async void CreateMatch_Tapped(string id, GestureSample gesture)
		{
			await battleManager.CreateBattle(teamManager.Team);
			statusPanel.Show("Match created!");
		}

		public void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			statusPanel.Background = content.PanelBackground;
			screenComponent = new ScreenComponent() { Background = content.MainBackground };
			screenComponent.Components.Add(startAsTester);
			screenComponent.Components.Add(teams);
			screenComponent.Components.Add(createMatch);
			screenComponent.Components.Add(joinMatch);
			screenComponent.Components.Add(myMatches);
			screenComponent.Components.Add(help);
			screenComponent.Components.Add(matchesPanel);
			screenComponent.Components.Add(helpPanel);
			screenComponent.Components.Add(statusPanel);

			helpPanel.Background = content.PanelBackground;
			helpPanel.Components.Add(new Text { Value = "Welcome to the MVP of GameThing!" });
			helpPanel.Components.Add(new Text { Value = "You play as a team of 5 characters." });
			helpPanel.Components.Add(new Text { Value = "Each character has a deck of 8 cards, with 4 cards in hand at a time." });
			helpPanel.Components.Add(new Text { Value = "Characters can play 2 cards and move 5 in their turn." });
			helpPanel.Components.Add(new Text { Value = "Once you play a card or move a character, you can't choose another this turn." });
			helpPanel.Components.Add(new Text { Value = "When you choose to be finished for a turn, press New Turn and your opponent will play." });
			helpPanel.Components.Add(new Text { Value = "After one side looses all characters, the other side wins!" });
			helpPanel.Components.Add(new Text { Value = "The MVP only has play versus another person and is turn-based." });

			screenComponent.GestureRead += ScreenComponent_GestureRead;
			screenComponent.LoadContent(content, graphicsDevice);

			battleManager.GetAvailableBattles().ContinueWith(task =>
			{
				availableBattles.AddRange(task.Result);
				SetDynamicButtons();
			});

			battleManager.GetMyBattles().ContinueWith(task => myBattles.AddRange(task.Result));
			backgroundLine = content.BackgroundLine;
		}

		public void ScreenComponent_GestureRead(GestureSample gesture)
		{
			helpPanel.IsVisible = false;
		}

		private void SetDynamicButtons()
		{
			matchesPanel.Components.Clear();
			var count = showingAvailableMatches ? availableBattles.Count : myBattles.Count;

			for (var i = 0; i < count; i++)
			{
				var battle = showingAvailableMatches ? availableBattles[i] : myBattles[i];
				var matchButton = new Button(GetBattleName(battle))
				{
					Id = battle.MatchId,
					Enabled = showingAvailableMatches
						? battle != null
						: battle?.HasBothSidesAdded == true,
					Tapped = MatchButton_Tapped
				};
				matchesPanel.Components.Add(matchButton);
			}
		}

		private string GetBattleName(BattleData battleData)
		{
			if (battleData == null)
				return "No match available.";

			// Return your player ID if only 1 player in battle
			if (battleData.Sides.Count == 1)
				return battleData.Sides.Keys.First();

			// Show "***" if it's your turn, and "v playerId" for opponent
			var yourPlayerId = ApplicationData.PlayerId;
			var yourTurn = battleData.CurrentPlayerId == yourPlayerId;
			return (yourTurn ? "*** " : "") + "v. " + battleData.Sides.Keys.First(playerId => playerId != yourPlayerId);
		}

		private async void MatchButton_Tapped(string matchId, GestureSample gesture)
		{
			BattleData battleData;
			if (showingAvailableMatches)
			{
				battleData = await battleManager.JoinBattleAndObserve(matchId, teamManager.Team);
				myBattles.Add(battleData);
				availableBattles.Remove(battleData);
				SetDynamicButtons();
			}
			else
			{
				battleData = await battleManager.GetBattleAndObserve(matchId);
			}

			StartBattle?.Invoke(battleData);
		}

		public void Update(GameTime gameTime)
		{
			while (TouchPanel.IsGestureAvailable)
			{
				var gesture = TouchPanel.ReadGesture();
				screenComponent.InvokeContainerGestureRead(gesture);

				if (gesture.GestureType == GestureType.Tap)
					screenComponent.InvokeContainerTap(gesture);
			}

			screenComponent.Update(gameTime);
		}

		public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			graphicsDevice.Clear(Color.White);

			spriteBatch.Begin();
			screenComponent.Draw(spriteBatch);

			startAsTester.Draw(spriteBatch);
			teams.Draw(spriteBatch);
			createMatch.Draw(spriteBatch);
			joinMatch.Draw(spriteBatch);
			myMatches.Draw(spriteBatch);
			help.Draw(spriteBatch);

			spriteBatch.Draw(backgroundLine, new Rectangle((graphicsDevice.Viewport.Width / 2) - 15, (int) (graphicsDevice.Viewport.Height * 0.125), 30, (int) (graphicsDevice.Viewport.Height * 0.75)), Color.White);

			matchesPanel.Draw(spriteBatch);
			helpPanel.Draw(spriteBatch);

			statusPanel.X = graphicsDevice.PresentationParameters.BackBufferWidth - (statusPanel.MeasureContent().X + (UIComponent.MARGIN * 20)) - UIComponent.MARGIN;  // draw from right
			statusPanel.Draw(spriteBatch);

			spriteBatch.End();
		}
	}
}
