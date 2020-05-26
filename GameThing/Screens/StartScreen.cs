using System.Collections.Generic;
using System.Linq;
using GameThing.Data;
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
		private const int AvailableMatchesCount = 6;

		private readonly Button startAsTester;
		private readonly Button teams;
		private readonly Button createMatch;
		private readonly Button joinMatch;
		private readonly Button myMatches;
		private readonly Button help;
		private ScreenComponent screenComponent;
		private readonly Panel matchesPanel;
		private readonly Panel helpPanel;

		private readonly TeamManager teamManager = TeamManager.Instance;
		private readonly BattleManager battleManager = BattleManager.Instance;
		private readonly List<BattleData> availableBattles = new List<BattleData>();
		private readonly List<BattleData> myBattles = new List<BattleData>();
		private bool showingAvailableMatches = true;
		private bool showingHelpDialog = false;

		public StartBattleEventHandler StartBattle;

		public StartScreen()
		{
			startAsTester = new Button("Test") { Tapped = startAsTester_Tapped };
			teams = new Button("Teams") { Tapped = teams_Tapped };
			createMatch = new Button("Create Match") { Tapped = createMatch_Tapped };
			joinMatch = new Button("Join Match") { Tapped = joinMatch_Tapped, Enabled = false };
			myMatches = new Button("My Matches") { Tapped = myMatches_Tapped };
			help = new Button("Help") { Tapped = help_Tapped };

			matchesPanel = new Panel() { ShowChrome = false };
			helpPanel = new Panel();

			BattleManager.Instance.DataUpdated += BattleManager_DataUpdated;
		}

		private void BattleManager_DataUpdated(BattleData battleData)
		{
			if (battleData.Status == BattleStatus.Finished)
				myBattles.Remove(battleData);
		}

		private void help_Tapped(string id, GestureSample gesture)
		{
			showingHelpDialog = true;
		}

		private void joinMatch_Tapped(string id, GestureSample gesture)
		{
			joinMatch.Enabled = false;
			myMatches.Enabled = true;
			showingAvailableMatches = true;
			SetDynamicButtons();
		}

		private void myMatches_Tapped(string id, GestureSample gesture)
		{
			joinMatch.Enabled = true;
			myMatches.Enabled = false;
			showingAvailableMatches = false;
			SetDynamicButtons();
		}

		private void startAsTester_Tapped(string id, GestureSample gesture)
		{
			var battleData = new BattleData { CurrentSidesTurn = CharacterSide.Spaghetti };
			battleData.InitializeCharacters("p1", teamManager.Team);
			battleData.ChangePlayingSide();
			battleData.InitializeCharacters("p2", teamManager.Team);

			var side = CharacterSide.Spaghetti;
			battleData.CurrentSidesTurn = side;
			battleData.IsTestMode = true;

			StartBattle?.Invoke(battleData);
		}

		private void teams_Tapped(string id, GestureSample gesture)
		{
			ApplicationData.CurrentScreen = ScreenType.ManageTeams;
		}

		private async void createMatch_Tapped(string id, GestureSample gesture)
		{
			var newBattle = await battleManager.CreateBattle(teamManager.Team);
			SetDynamicButtons();
		}

		public void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			screenComponent = new ScreenComponent(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight) { Background = content.MainBackground };
			screenComponent.Components.Add(startAsTester);
			screenComponent.Components.Add(teams);
			screenComponent.Components.Add(createMatch);
			screenComponent.Components.Add(joinMatch);
			screenComponent.Components.Add(myMatches);
			screenComponent.Components.Add(help);
			screenComponent.Components.Add(matchesPanel);
			screenComponent.Components.Add(helpPanel);

			for (int i = 0; i < AvailableMatchesCount; i++)
				matchesPanel.Components.Add(new Button("No match available.") { Tapped = matchButton_Tapped });

			helpPanel.Components.Add(new Text { Value = "Welcome to the MVP of GameThing!" });
			helpPanel.Components.Add(new Text { Value = "You play as a team of 5 characters." });
			helpPanel.Components.Add(new Text { Value = "Each character has a deck of 8 cards, with 4 cards in hand at a time." });
			helpPanel.Components.Add(new Text { Value = "Characters can play 2 cards and move 5 in their turn." });
			helpPanel.Components.Add(new Text { Value = "Once you play a card or move a character, you can't choose another this turn." });
			helpPanel.Components.Add(new Text { Value = "When you choose to be finished for a turn, press New Turn and your opponent will play." });
			helpPanel.Components.Add(new Text { Value = "After one side looses all characters, the other side wins!" });
			helpPanel.Components.Add(new Text { Value = "The MVP only has play versus another person and is turn-based." });

			screenComponent.GestureRead += screenComponent_GestureRead;
			screenComponent.LoadContent(content, graphicsDevice);

			battleManager.GetAvailableBattles().ContinueWith(task =>
			{
				availableBattles.AddRange(task.Result);
				SetDynamicButtons();
			});

			battleManager.GetMyBattles().ContinueWith(task => myBattles.AddRange(task.Result));
		}

		public void screenComponent_GestureRead(GestureSample gesture)
		{
			showingHelpDialog = false;
		}

		private void SetDynamicButtons()
		{
			for (int i = 0; i < AvailableMatchesCount; i++)
			{
				var matchButton = (Button) matchesPanel.Components[i];
				var battle = showingAvailableMatches
					? (i < availableBattles.Count ? availableBattles[i] : null)
					: (i < myBattles.Count ? myBattles[i] : null);

				matchButton.Id = battle?.MatchId;
				matchButton.Text = GetBattleName(battle);
				matchButton.Enabled = showingAvailableMatches
					? battle != null
					: battle?.HasBothSidesAdded == true;
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
			var yourTurn = battleData.CurrentSidesTurn == battleData.Sides[yourPlayerId];
			return (yourTurn ? "*** " : "") + "v. " + battleData.Sides.Keys.First(playerId => playerId != yourPlayerId);
		}

		private async void matchButton_Tapped(string matchId, GestureSample gesture)
		{
			BattleData battleData = null;
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
		}

		public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			graphicsDevice.Clear(Color.White);

			spriteBatch.Begin();
			screenComponent.Draw(spriteBatch);

			startAsTester.DrawConditional(spriteBatch, 450, 200, teamManager.HasTeam);
			teams.Draw(spriteBatch, 450, 320);
			createMatch.DrawConditional(spriteBatch, 450, 440, teamManager.HasTeam);
			joinMatch.DrawConditional(spriteBatch, 450, 560, teamManager.HasTeam);
			myMatches.DrawConditional(spriteBatch, 450, 680, teamManager.HasTeam);
			help.Draw(spriteBatch, 450, 800);

			matchesPanel.DrawConditional(spriteBatch, 1100, 168, teamManager.HasTeam);
			helpPanel.DrawConditional(spriteBatch, 400, 300, showingHelpDialog);

			spriteBatch.End();
		}
	}
}
