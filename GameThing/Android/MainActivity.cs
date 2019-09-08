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
using Android.OS;
using Android.Runtime;
using Android.Views;
using GameThing.Android.BaseGameUtils;
using GameThing.Data;

namespace GameThing.Android
{
	[Activity(Label = "GameThing", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/AppTheme", AlwaysRetainTaskState = true, LaunchMode = LaunchMode.SingleInstance,
		ScreenOrientation = ScreenOrientation.Landscape, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
	public class MainActivity : BaseGameActivity, IGameHelperListener
	{
		private const int RC_SELECT_PLAYERS = 0;
		private const int RC_MATCH_INBOX = 1;
		private const int RC_SIGN_IN = 2;
		private TurnBasedMultiplayerClient gameClient;
		private GoogleSignInClient googleSignInClient;
		private GoogleSignInAccount googleSignInAccount;
		private MainGame game;


		protected override void OnCreate(Bundle bundle)
		{
			Window.AddFlags(WindowManagerFlags.Fullscreen);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);
			Window.AddFlags(WindowManagerFlags.TurnScreenOn);

			int uiOptions = (int) Window.DecorView.SystemUiVisibility;
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

			GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultGamesSignIn).Build();
			googleSignInClient = GoogleSignIn.GetClient(this, gso);
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
			StartActivityForResult(intent, RC_MATCH_INBOX);
		}

		private async void Game_CreateMatch()
		{
			var intent = await gameClient.GetSelectOpponentsIntent(1, 1).AsAsync<Intent>();
			StartActivityForResult(intent, RC_SELECT_PLAYERS);
		}

		private void Game_NextPlayersTurn(BattleData data)
		{
			StartNextTurn(data);
		}

		private void Game_RequestSignIn()
		{
			beginUserInitiatedSignIn();

			RequestSignIn();
		}

		private void Game_GameOver(BattleData data)
		{
			if (data.MatchId == null)
				return;

			var gameData = Convert.Serialize(data);
			var results = data
				.Sides
				.Select(keyValuePair => new ParticipantResult(
					keyValuePair.Key,
					data.WinnerParticipantId == keyValuePair.Key ? ParticipantResult.MatchResultWin : ParticipantResult.MatchResultLoss,
					data.WinnerParticipantId == keyValuePair.Key ? 1 : 2))
				.ToList();

			gameClient.FinishMatch(data.MatchId, gameData, results);
		}

		private void StartNextTurn(BattleData data)
		{
			if (data.MatchId == null)
				return;

			var gameData = Convert.Serialize(data);
			var participantId = data.GetParticipantIdForCurrentSide();
			gameClient.TakeTurn(data.MatchId, gameData, participantId);
		}

		protected override async void OnActivityResult(int request, Result response, Intent data)
		{
			base.OnActivityResult(request, response, data);

			if (response == Result.Canceled)
				return;

			if (request == RC_SELECT_PLAYERS)
				await AddPlayers(data);
			if (request == RC_MATCH_INBOX)
				AcceptInvitation(data);
			if (request == RC_SIGN_IN)
				await SignInSucceded(data);
		}

		private void RequestSignIn()
		{
			StartActivityForResult(googleSignInClient.SignInIntent, RC_SIGN_IN);
		}

		private async Task SignInSucceded(Intent data)
		{
			googleSignInAccount = await GoogleSignIn.GetSignedInAccountFromIntent(data).AsAsync<GoogleSignInAccount>();
			gameClient = GamesClass.GetTurnBasedMultiplayerClient(this, googleSignInAccount);
			game.SetSignedIn(true);

			var match = getTurnBasedMatch();
			if (match != null)
				StartOrEndMatch(match);
		}

		private async Task AddPlayers(Intent data)
		{
			var invitees = data.GetStringArrayListExtra(GamesClass.ExtraPlayerIds);
			var config = TurnBasedMatchConfig
				.InvokeBuilder()
				.AddInvitedPlayer(invitees[0])
				.Build();

			var match = await gameClient.CreateMatch(config).AsAsync<TurnBasedMatchEntity>();
			var gameData = match.GetData();
			if (gameData == null)
			{
				// This is the first turn, need to initialize game data.
				var battleData = game.InitializeGameData(match.MatchId, match.ParticipantIds);

				// Send this data to the cloud so user can't restart match for better cards
				StartNextTurn(battleData);

				setTurnBasedMatch(match);
				game.StartMatch(GetMyParticipantId(), battleData);
			}
			else
			{
				game.StartMatch(GetMyParticipantId(), Convert.Deserialize<BattleData>(gameData));
			}
		}

		private void AcceptInvitation(Intent data)
		{
			var match = data.GetParcelableExtra(Multiplayer.ExtraTurnBasedMatch).JavaCast<ITurnBasedMatch>();
			setTurnBasedMatch(match);

			StartOrEndMatch(match);
		}

		private void StartOrEndMatch(ITurnBasedMatch match)
		{
			var battleData = Convert.Deserialize<BattleData>(match.GetData());

			if (match.Status == TurnBasedMatch.MatchStatusComplete)
				game.ShowGameOver(battleData);
			else
				game.StartMatch(GetMyParticipantId(), battleData);
		}

		private string GetMyParticipantId()
		{
			var playerId = GamesClass.Players.GetCurrentPlayerId(getApiClient());
			return getTurnBasedMatch().GetParticipantId(playerId);
		}
	}
}
