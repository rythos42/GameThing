using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Extensions;
using Android.Gms.Games;
using Android.Gms.Games.MultiPlayer;
using Android.Gms.Games.MultiPlayer.TurnBased;
using Android.Gms.Games.Snapshot;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GameThing.Android.BaseGameUtils;
using GameThing.Data;

namespace GameThing.Android
{
	[Activity(Label = "GameThing", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/AppTheme", AlwaysRetainTaskState = true, LaunchMode = LaunchMode.SingleInstance,
		ScreenOrientation = ScreenOrientation.Landscape, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
	public class MainActivity : BaseGameActivity, IGameHelperListener
	{
		private const int requestCode_SelectPlayers = 0;
		private const int requestCode_MatchInbox = 1;
		private const int requestCode_SignIn = 2;

		private TurnBasedMultiplayerClient gameClient;
		private SnapshotsClient savedGamesClient;
		private ISnapshot teamSnapshot;

		private GoogleSignInClient googleSignInClient;
		private GoogleSignInAccount googleSignInAccount;
		private MainGame game;

		protected override void OnCreate(Bundle bundle)
		{
			Window.AddFlags(WindowManagerFlags.Fullscreen);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);
			Window.AddFlags(WindowManagerFlags.TurnScreenOn);

			var uiOptions = (int) Window.DecorView.SystemUiVisibility;
			uiOptions |= (int) SystemUiFlags.Fullscreen;
			uiOptions |= (int) SystemUiFlags.HideNavigation;
			uiOptions |= (int) SystemUiFlags.ImmersiveSticky;
			Window.DecorView.SystemUiVisibility = (StatusBarVisibility) uiOptions;

			base.OnCreate(bundle);

			game = new MainGame();
			game.CreateMatch += Game_CreateMatch;
			game.JoinMatch += Game_JoinMatch;
			game.NextPlayersTurn += Game_NextPlayersTurn;
			game.RequestSignIn += Game_RequestSignIn;
			game.GameOver += Game_GameOver;
			SetContentView((View) game.Services.GetService(typeof(View)));
			game.Run();

			using (var builder = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultGamesSignIn))
			{
				googleSignInClient = GoogleSignIn.GetClient(this, builder.Build());
			}
		}

		public void OnSignInFailed()
		{
			// Looks like BaseGameActivity shows a dialog if failed, so I don't need to do anything yet.
		}

		public void OnSignInSucceeded()
		{
			RequestSignIn();
		}

		private async void Game_JoinMatch()
		{
			var intent = await gameClient.GetInboxIntent().AsAsync<Intent>();
			StartActivityForResult(intent, requestCode_MatchInbox);
		}

		private async void Game_CreateMatch()
		{
			var intent = await gameClient.GetSelectOpponentsIntent(1, 1).AsAsync<Intent>();
			StartActivityForResult(intent, requestCode_SelectPlayers);
		}

		private void Game_NextPlayersTurn(BattleData data)
		{
			SendDataToNextPlayer(data);
		}

		private void Game_RequestSignIn()
		{
			BeginUserInitiatedSignIn();

			RequestSignIn();
		}

		private void Game_GameOver(string matchId, TeamData teamData)
		{
			gameClient.FinishMatch(matchId);

			teamSnapshot.SnapshotContents.WriteBytes(Convert.Serialize(teamData));
			using (var builder = new SnapshotMetadataChangeBuilder())
			{
				var metadataChange = builder
					.SetDescription("")
					.SetPlayedTimeMillis(0)
					.SetProgressValue(0)
					.Build();

				savedGamesClient.CommitAndClose(teamSnapshot, metadataChange);
				LoadTeamFromGoogleDrive();
			}
		}

		protected override async void OnActivityResult(int request, Result response, Intent data)
		{
			base.OnActivityResult(request, response, data);

			if (response == Result.Canceled)
				return;

			if (request == requestCode_SelectPlayers)
				await AddPlayers(data);
			if (request == requestCode_MatchInbox)
				StartFromGooglePlayGames(data);
			if (request == requestCode_SignIn)
				await SignInSucceded(data);
		}

		private void RequestSignIn()
		{
			StartActivityForResult(googleSignInClient.SignInIntent, requestCode_SignIn);
		}

		private async Task SignInSucceded(Intent data)
		{
			googleSignInAccount = await GoogleSignIn.GetSignedInAccountFromIntent(data).AsAsync<GoogleSignInAccount>();
			gameClient = GamesClass.GetTurnBasedMultiplayerClient(this, googleSignInAccount);
			game.SetSignedIn(true);
			savedGamesClient = GamesClass.GetSnapshotsClient(this, googleSignInAccount);

			await LoadTeamFromGoogleDrive();

			var match = GetTurnBasedMatch();
			if (match != null)
			{
				var battleData = Convert.Deserialize<BattleData>(match.GetData());
				StartOrEndMatch(match, battleData);
			}
		}

		private async Task LoadTeamFromGoogleDrive()
		{
			var savedGameOrConflict = await savedGamesClient.Open("GameThingTeam.json", true, SnapshotsClient.ResolutionPolicyMostRecentlyModified).AsAsync<SnapshotsClient.DataOrConflict>();
			teamSnapshot = savedGameOrConflict.Data as ISnapshot;
			if (teamSnapshot == null)
			{
				game.TeamData = TeamData.CreateDefaultTeam();
			}
			else
			{
				var teamDataBytes = teamSnapshot.SnapshotContents.ReadFully();
				game.TeamData = teamDataBytes.Length != 0 ? Convert.Deserialize<TeamData>(teamDataBytes) : TeamData.CreateDefaultTeam();
			}
		}

		private async Task AddPlayers(Intent data)
		{
			var invitees = data.GetStringArrayListExtra(GamesClass.ExtraPlayerIds);
			var config = TurnBasedMatchConfig
				.InvokeBuilder()
				.AddInvitedPlayer(invitees[0])
				.Build();

			var match = await gameClient.CreateMatch(config) as TurnBasedMatchEntity;
			SetTurnBasedMatch(match);

			if (match.GetData() == null)
			{
				// This is the first turn, need to initialize game data.
				var battleData = game.InitializeBattleData(match.MatchId);
				var myParticipantId = GetMyParticipantId();
				battleData.InitializeCharacters(myParticipantId, game.TeamData);
				battleData.ChangePlayingSide();

				var nextPlayerParticipantId = match.ParticipantIds.SingleOrDefault(participantId => participantId != myParticipantId);
				SendDataToNextPlayer(battleData, nextPlayerParticipantId);

				Toast.MakeText(Application.Context, $"Your friend has been invited to play!", ToastLength.Long).Show();
			}
		}

		private void SendDataToNextPlayer(BattleData data, string participantId = null)
		{
			var gameData = Convert.Serialize(data);
			participantId = participantId ?? data.GetParticipantIdForCurrentSide();
			gameClient.TakeTurn(data.MatchId, gameData, participantId);
		}

		private void StartFromGooglePlayGames(Intent data)
		{
			var match = data.GetParcelableExtra(Multiplayer.ExtraTurnBasedMatch).JavaCast<ITurnBasedMatch>();
			SetTurnBasedMatch(match);

			var battleData = Convert.Deserialize<BattleData>(match.GetData());
			var participantId = GetMyParticipantId();

			if (!battleData.HasBothSidesAdded)
			{
				if (battleData.HasMySideAdded(participantId))
				{
					Toast.MakeText(Application.Context, "Your opponent hasn't joined yet.", ToastLength.Long).Show();
					return;
				}
				battleData.InitializeCharacters(participantId, game.TeamData);
			}

			StartOrEndMatch(match, battleData);
		}

		private void StartOrEndMatch(ITurnBasedMatch match, BattleData battleData)
		{
			if (match.Status == TurnBasedMatch.MatchStatusComplete)
				game.ShowGameOver(battleData);
			else
				game.StartMatch(GetMyParticipantId(), battleData);
		}

		private string GetMyParticipantId()
		{
			var playerId = GamesClass.Players.GetCurrentPlayerId(GetApiClient());
			return GetTurnBasedMatch().GetParticipantId(playerId);
		}
	}
}
