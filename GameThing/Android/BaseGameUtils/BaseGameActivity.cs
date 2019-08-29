using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Games.MultiPlayer.TurnBased;
using Android.OS;
using Exception = System.Exception;
using String = System.String;

namespace GameThing.Android.BaseGameUtils
{
	[Activity(Label = "BaseGameActivity")]
	/**
     * Example base class for games. This implementation takes care of setting up
     * the API client object and managing its lifecycle. Subclasses only need to
     * override the @link{#OnSignInSucceeded} and @link{#OnSignInFailed} abstract
     * methods. To initiate the sign-in flow when the user clicks the sign-in
     * button, subclasses should call @link{#beginUserInitiatedSignIn}. By default,
     * this class only instantiates the GoogleApiClient object. If the PlusClient or
     * AppStateClient objects are also wanted, call the BaseGameActivity(int)
     * constructor and specify the requested clients. For example, to request
     * PlusClient and GamesClient, use BaseGameActivity(CLIENT_GAMES | CLIENT_PLUS).
     * To request all available clients, use BaseGameActivity(CLIENT_ALL).
     * Alternatively, you can also specify the requested clients via
     * @link{#setRequestedClients}, but you must do so before @link{#onCreate}
     * gets called, otherwise the call will have no effect.
     *
     * @author Bruno Oliveira (Google)
     */
	public abstract class BaseGameActivity : Microsoft.Xna.Framework.AndroidGameActivity
	{
		public static GameHelper mHelper;
		protected bool mDebugLog = true;
		protected int mRequestedClients = GameHelper.CLIENT_GAMES;

		protected BaseGameActivity()
		{
		}

		public GameHelper getGameHelper()
		{
			try
			{

				if (mHelper == null)
				{
					mHelper = new GameHelper(this, mRequestedClients);
					mHelper.enableDebugLog(mDebugLog);
				}
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
			return mHelper;
		}

		protected override void OnCreate(Bundle b)
		{
			try
			{
				base.OnCreate(b);
				if (mHelper == null)
					getGameHelper();
				mHelper.setup(this as IGameHelperListener);
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}

		protected override void OnStart()
		{
			try
			{
				base.OnStart();
				mHelper.onStart(this);
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}

		protected override void OnStop()
		{
			base.OnStop();
			mHelper.onStop();
		}

		protected override void OnActivityResult(int request, Result response, Intent data)
		{
			base.OnActivityResult(request, response, data);
			mHelper.onActivityResult(request, (int) response, data);
		}

		protected GoogleApiClient getApiClient()
		{
			return mHelper.getApiClient();
		}

		protected bool isSignedIn()
		{
			return mHelper.isSignedIn();
		}

		protected void beginUserInitiatedSignIn()
		{
			mHelper.beginUserInitiatedSignIn();
		}

		protected void showAlert(String message)
		{
			mHelper.makeSimpleDialog(message).Show();
		}

		protected void showAlert(String title, String message)
		{
			mHelper.makeSimpleDialog(title, message).Show();
		}

		protected void enableDebugLog(bool enabled)
		{
			mDebugLog = true;
			if (mHelper != null)
				mHelper.enableDebugLog(enabled);
		}

		protected bool hasSignInError()
		{
			return mHelper.hasSignInError();
		}

		protected SignInFailureReason getSignInError()
		{
			return mHelper.getSignInError();
		}

		protected void setTurnBasedMatch(ITurnBasedMatch turnBasedMatch)
		{
			mHelper.setTurnBasedMatch(turnBasedMatch);
		}

		protected ITurnBasedMatch getTurnBasedMatch()
		{
			return mHelper.getTurnBasedMatch();
		}
	}
}
