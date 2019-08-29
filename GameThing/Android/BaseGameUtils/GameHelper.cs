using System;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Games;
using Android.Gms.Games.MultiPlayer;
using Android.Gms.Games.MultiPlayer.TurnBased;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Java.Lang;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using String = System.String;

namespace GameThing.Android.BaseGameUtils
{
	public class GameHelper : Object, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
	{
		public static String TAG = "GameHelper";

		private static readonly int RC_RESOLVE = 9001;              // Request code we use when invoking other Activities to complete the sign-in flow.
		private static readonly int RC_UNUSED = 9002;               // Request code when invoking Activities whose result we don't care about.

		public static int CLIENT_GAMES = 0x01;

		private static readonly int DEFAULT_MAX_SIGN_IN_ATTEMPTS = 3;
		private readonly Handler mHandler;

		private readonly int mRequestedClients;
		private readonly String GAMEHELPER_SHARED_PREFS = "GAMEHELPER_SHARED_PREFS";
		private readonly String KEY_SIGN_IN_CANCELLATIONS = "KEY_SIGN_IN_CANCELLATIONS";
		private Activity mActivity;

		private Context mAppContext;
		private Api.ApiOptionsNoOptions mAppStateApiOptions;

		// Whether to automatically try to sign in on onStart(). We only set this
		// to true when the sign-in process fails or the user explicitly signs out.
		// We set it back to false when the user initiates the sign in process.
		private bool mConnectOnStart = true;
		private bool mConnecting;

		// The connection result we got from our last attempt to sign-in.
		private ConnectionResult mConnectionResult;

		private bool mDebugLog;
		private bool mExpectingResolution;
		private GamesClass.GamesOptions mGamesApiOptions = GamesClass.GamesOptions.InvokeBuilder().Build();
		private GoogleApiClient mGoogleApiClient;
		private GoogleApiClient.Builder mGoogleApiClientBuilder;
		private IGameHelperListener mListener;

		// Should we start the flow to sign the user in automatically on startup? If so, up to how many times in the life of the application?
		private int mMaxAutoSignInAttempts = DEFAULT_MAX_SIGN_IN_ATTEMPTS;
		private bool mSetupDone;
		private bool mShowErrorDialogs = true;
		private bool mSignInCancelled;
		private SignInFailureReason mSignInFailureReason;
		private ITurnBasedMatch mTurnBasedMatch;
		private bool mUserInitiatedSignIn;

		public GameHelper(Activity activity, int clientsToUse)
		{
			mActivity = activity;
			mAppContext = activity.ApplicationContext;
			mRequestedClients = clientsToUse;
			mHandler = new Handler();
		}

		public void OnConnected(Bundle connectionHint)
		{
			debugLog("onConnected: connected!");

			if (connectionHint != null)
			{
				debugLog("onConnected: connection hint provided. Checking for TBMP game.");
				mTurnBasedMatch = connectionHint.GetParcelable(Multiplayer.ExtraTurnBasedMatch).JavaCast<ITurnBasedMatch>();
			}

			// we're good to go
			succeedSignIn();
		}

		public void OnConnectionSuspended(int cause)
		{
			debugLog("onConnectionSuspended, cause=" + cause);
			disconnect();
			mSignInFailureReason = null;
			debugLog("Making extraordinary call to OnSignInFailed callback");
			mConnecting = false;
			notifyListener(false);
		}

		public void OnConnectionFailed(ConnectionResult result)
		{
			// save connection result for later reference
			debugLog("onConnectionFailed");

			mConnectionResult = result;
			debugLog("Connection failure:");
			debugLog("   - code: " + GameHelperUtils.errorCodeToString(mConnectionResult.ErrorCode));
			debugLog("   - resolvable: " + mConnectionResult.HasResolution);
			debugLog("   - details: " + mConnectionResult);

			int cancellations = getSignInCancellations();
			bool shouldResolve = false;

			if (mUserInitiatedSignIn)
			{
				debugLog("onConnectionFailed: WILL resolve because user initiated sign-in.");
				shouldResolve = true;
			}
			else if (mSignInCancelled)
			{
				debugLog("onConnectionFailed WILL NOT resolve (user already cancelled once).");
				shouldResolve = false;
			}
			else if (cancellations < mMaxAutoSignInAttempts)
			{
				debugLog("onConnectionFailed: WILL resolve because we have below the max# of "
						 + "attempts, "
						 + cancellations
						 + " < "
						 + mMaxAutoSignInAttempts);
				shouldResolve = true;
			}
			else
			{
				shouldResolve = false;
				debugLog("onConnectionFailed: Will NOT resolve; not user-initiated and max attempts "
						 + "reached: "
						 + cancellations
						 + " >= "
						 + mMaxAutoSignInAttempts);
			}

			if (!shouldResolve)
			{
				// Fail and wait for the user to want to sign in.
				debugLog("onConnectionFailed: since we won't resolve, failing now.");
				mConnectionResult = result;
				mConnecting = false;
				notifyListener(false);
				return;
			}

			debugLog("onConnectionFailed: resolving problem...");

			// Resolve the connection result. This usually means showing a dialog or
			// starting an Activity that will allow the user to give the appropriate
			// consents so that sign-in can be successful.
			resolveConnectionResult();
		}

		public void setMaxAutoSignInAttempts(int max)
		{
			mMaxAutoSignInAttempts = max;
		}

		private void assertConfigured(String operation)
		{
			if (!mSetupDone)
			{
				var error = "GameHelper error: Operation attempted without setup: "
							   + operation
							   + ". The setup() method must be called before attempting any other operation.";
				logError(error);
				throw new IllegalStateException(error);
			}
		}

		private void doApiOptionsPreCheck()
		{
			if (mGoogleApiClientBuilder != null)
			{
				var error = "GameHelper: you cannot call set*ApiOptions after the client "
							   + "builder has been created. Call it before calling CreateApiClientBuilder() "
							   + "or setup().";
				logError(error);
				throw new IllegalStateException(error);
			}
		}

		public void setGamesApiOptions(GamesClass.GamesOptions options)
		{
			doApiOptionsPreCheck();
			mGamesApiOptions = options;
		}

		public void setAppStateApiOptions(Api.ApiOptionsNoOptions options)
		{
			doApiOptionsPreCheck();
			mAppStateApiOptions = options;
		}

		public GoogleApiClient.Builder CreateApiClientBuilder()
		{
			try
			{
				if (mSetupDone)
				{
					var error = "GameHelper: you called GameHelper.CreateApiClientBuilder() after " +
								   "calling setup. You can only get a client builder BEFORE performing setup.";
					logError(error);
					throw new IllegalStateException(error);
				}

				var builder = new GoogleApiClient.Builder(mActivity, this, this);

				if (0 != (mRequestedClients & CLIENT_GAMES))
				{
					try
					{
						//TODO: FIX
						builder.AddApi(GamesClass.API, mGamesApiOptions);
					}
					catch (Exception ex)
					{
						ex.ToString();
						builder.AddApi(GamesClass.API);
					}
					builder.AddScope(GamesClass.ScopeGames);

				}

				mGoogleApiClientBuilder = builder;
				return builder;
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
			return null;
		}

		public void setup(IGameHelperListener listener)
		{
			try
			{
				if (mSetupDone)
				{
					var error = "GameHelper: you cannot call GameHelper.setup() more than once!";
					logError(error);
					throw new IllegalStateException(error);
				}
				mListener = listener ?? throw new Exception("The listener is not of type IGameHelperListener");
				debugLog("Setup: requested clients: " + mRequestedClients);

				if (mGoogleApiClientBuilder == null)
				{
					// we don't have a builder yet, so create one
					mGoogleApiClientBuilder = CreateApiClientBuilder();
				}

				mGoogleApiClient = mGoogleApiClientBuilder.Build();
				mGoogleApiClientBuilder = null;
				mSetupDone = true;
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}

		public GoogleApiClient getApiClient()
		{
			if (mGoogleApiClient == null)
			{
				throw new IllegalStateException("No GoogleApiClient. Did you call setup()?");
			}
			return mGoogleApiClient;
		}

		public bool isSignedIn()
		{
			return mGoogleApiClient != null && mGoogleApiClient.IsConnected;
		}

		public bool isConnecting()
		{
			return mConnecting;
		}

		public bool hasSignInError()
		{
			return mSignInFailureReason != null;
		}

		public SignInFailureReason getSignInError()
		{
			return mSignInFailureReason;
		}

		// Set whether to show error dialogs or not.
		public void setShowErrorDialogs(bool show)
		{
			mShowErrorDialogs = show;
		}

		/** Call this method from your Activity's onStart(). */
		public void onStart(Activity act)
		{
			try
			{
				mActivity = act;
				mAppContext = act.ApplicationContext;

				debugLog("onStart");
				assertConfigured("onStart");

				if (mConnectOnStart)
				{
					if (mGoogleApiClient.IsConnected)
					{
						Log.Warn(TAG,
							"GameHelper: client was already connected on onStart()");
					}
					else
					{
						debugLog("Connecting client.");
						mConnecting = true;
						mGoogleApiClient.Connect();
					}
				}
				else
				{
					debugLog("Not attempting to connect becase mConnectOnStart=false");
					debugLog("Instead, reporting a sign-in failure.");
					var runnable = new Runnable(() => { notifyListener(false); });
					mHandler.PostDelayed(runnable, 1000);
				}
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}

		/** Call this method from your Activity's onStop(). */
		public void onStop()
		{
			debugLog("onStop");
			assertConfigured("onStop");
			if (mGoogleApiClient.IsConnected)
			{
				debugLog("Disconnecting client due to onStop");
				mGoogleApiClient.Disconnect();
			}
			else
			{
				debugLog("Client already disconnected when we got onStop.");
			}
			mConnecting = false;
			mExpectingResolution = false;

			// let go of the Activity reference
			mActivity = null;
		}

		public bool hasTurnBasedMatch()
		{
			return mTurnBasedMatch != null;
		}

		public void clearTurnBasedMatch()
		{
			mTurnBasedMatch = null;
		}

		public void setTurnBasedMatch(ITurnBasedMatch turnBasedMatch)
		{
			mTurnBasedMatch = turnBasedMatch;
		}

		public ITurnBasedMatch getTurnBasedMatch()
		{
			if (!mGoogleApiClient.IsConnected)
			{
				Log.Warn(TAG,
					"Warning: getTurnBasedMatch() should only be called when signed in, "
					+ "that is, after getting onSignInSuceeded()");
			}
			return mTurnBasedMatch;
		}

		public void enableDebugLog(bool enabled)
		{
			mDebugLog = enabled;
			if (enabled)
			{
				debugLog("Debug log enabled.");
			}
		}

		public void onActivityResult(int requestCode, int responseCode, Intent intent)
		{
			try
			{
				debugLog("onActivityResult: req="
						 + (requestCode == RC_RESOLVE ? "RC_RESOLVE" : Java.Lang.String.ValueOf(requestCode)) +
						 ", resp="
						 + GameHelperUtils.activityResponseCodeToString(responseCode));
				if (requestCode != RC_RESOLVE)
				{
					debugLog("onActivityResult: request code not meant for us. Ignoring.");
					return;
				}

				// no longer expecting a resolution
				mExpectingResolution = false;

				if (!mConnecting)
				{
					debugLog("onActivityResult: ignoring because we are not connecting.");
					return;
				}

				// We're coming back from an activity that was launched to resolve a connection problem. For example, the sign-in UI.
				if (responseCode == (int) Result.Ok)
				{
					// Ready to try to connect again.
					debugLog("onAR: Resolution was RESULT_OK, so connecting current client again.");
					connect();
				}
				else if (responseCode == GamesActivityResultCodes.ResultReconnectRequired)
				{
					debugLog("onAR: Resolution was RECONNECT_REQUIRED, so reconnecting.");
					connect();
				}
				else if (responseCode == (int) Result.Canceled)
				{
					// User cancelled.
					debugLog("onAR: Got a cancellation result, so disconnecting.");
					mSignInCancelled = true;
					mConnectOnStart = false;
					mUserInitiatedSignIn = false;
					mSignInFailureReason = null; // cancelling is not a failure!
					mConnecting = false;
					mGoogleApiClient.Disconnect();

					// increment # of cancellations
					int prevCancellations = getSignInCancellations();
					int newCancellations = incrementSignInCancellations();
					debugLog("onAR: # of cancellations " + prevCancellations + " --> " + newCancellations + ", max " + mMaxAutoSignInAttempts);

					notifyListener(false);
				}
				else
				{
					// Whatever the problem we were trying to solve, it was not solved. So give up and show an error message.
					debugLog("onAR: responseCode=" + GameHelperUtils.activityResponseCodeToString(responseCode) + ", so giving up.");
					giveUp(new SignInFailureReason(mConnectionResult.ErrorCode, responseCode));
				}
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}

		private void notifyListener(bool success)
		{
			try
			{
				debugLog("Notifying LISTENER of sign-in "
						 + (success ? "SUCCESS" : mSignInFailureReason != null ? "FAILURE (error)" : "FAILURE (no error)"));
				if (mListener != null)
				{
					if (success)
					{
						mListener.OnSignInSucceeded();
					}
					else
					{
						mListener.OnSignInFailed();
					}
				}
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}

		public void beginUserInitiatedSignIn()
		{
			debugLog("beginUserInitiatedSignIn: resetting attempt count.");
			resetSignInCancellations();
			mSignInCancelled = false;
			mConnectOnStart = true;

			if (mGoogleApiClient.IsConnected)
			{
				// nothing to do
				logWarn("beginUserInitiatedSignIn() called when already connected. "
						+ "Calling listener directly to notify of success.");
				notifyListener(true);
				return;
			}
			if (mConnecting)
			{
				logWarn("beginUserInitiatedSignIn() called when already connecting. "
						+ "Be patient! You can only call this method after you get an "
						+ "OnSignInSucceeded() or OnSignInFailed() callback. Suggestion: disable "
						+ "the sign-in button on startup and also when it's clicked, and re-enable "
						+ "when you get the callback.");
				// ignore call (listener will get a callback when the connection process finishes)
				return;
			}

			debugLog("Starting USER-INITIATED sign-in flow.");

			// indicate that user is actively trying to sign in (so we know to resolve connection problems by showing dialogs)
			mUserInitiatedSignIn = true;

			if (mConnectionResult != null)
			{
				// We have a pending connection result from a previous failure, so start with that.
				debugLog("beginUserInitiatedSignIn: continuing pending sign-in flow.");
				mConnecting = true;
				resolveConnectionResult();
			}
			else
			{
				// We don't have a pending connection result, so start anew.
				debugLog("beginUserInitiatedSignIn: starting new sign-in flow.");
				mConnecting = true;
				connect();
			}
		}

		private void connect()
		{
			if (mGoogleApiClient.IsConnected)
			{
				debugLog("Already connected.");
				return;
			}
			debugLog("Starting connection.");
			mConnecting = true;
			mTurnBasedMatch = null;
			mGoogleApiClient.Connect();
		}

		private void succeedSignIn()
		{
			debugLog("succeedSignIn");
			mSignInFailureReason = null;
			mConnectOnStart = true;
			mUserInitiatedSignIn = false;
			mConnecting = false;
			notifyListener(true);
		}

		// Return the number of times the user has cancelled the sign-in flow in the life of the app
		private int getSignInCancellations()
		{
			ISharedPreferences sp = mAppContext.GetSharedPreferences(GAMEHELPER_SHARED_PREFS, FileCreationMode.Private);
			return sp.GetInt(KEY_SIGN_IN_CANCELLATIONS, 0);
		}

		// Increments the counter that indicates how many times the user has cancelled the sign in flow in the life of the application
		private int incrementSignInCancellations()
		{
			int cancellations = getSignInCancellations();
			ISharedPreferencesEditor editor =
				mAppContext.GetSharedPreferences(GAMEHELPER_SHARED_PREFS, FileCreationMode.Private).Edit();
			editor.PutInt(KEY_SIGN_IN_CANCELLATIONS, cancellations + 1);
			editor.Commit();
			return cancellations + 1;
		}

		// Reset the counter of how many times the user has cancelled the sign-in flow.
		private void resetSignInCancellations()
		{
			ISharedPreferencesEditor editor =
				mAppContext.GetSharedPreferences(GAMEHELPER_SHARED_PREFS, FileCreationMode.Private).Edit();
			editor.PutInt(KEY_SIGN_IN_CANCELLATIONS, 0);
			editor.Commit();
		}

		private void resolveConnectionResult()
		{
			try
			{
				// Try to resolve the problem
				if (mExpectingResolution)
				{
					debugLog("We're already expecting the result of a previous resolution.");
					return;
				}

				debugLog("resolveConnectionResult: trying to resolve result: "
						 + mConnectionResult);
				if (mConnectionResult.HasResolution)
				{
					// This problem can be fixed. So let's try to fix it.
					debugLog("Result has resolution. Starting it.");
					try
					{
						// launch appropriate UI flow (which might, for example, be the sign-in flow)
						mExpectingResolution = true;
						mConnectionResult.StartResolutionForResult(mActivity, RC_RESOLVE);
					}
					catch (IntentSender.SendIntentException e)
					{
						e.ToString();
						// Try connecting again
						debugLog("SendIntentException, so connecting again.");
						connect();
					}
				}
				else
				{
					// It's not a problem what we can solve, so give up and show an error.
					debugLog("resolveConnectionResult: result has no resolution. Giving up.");
					giveUp(new SignInFailureReason(mConnectionResult.ErrorCode));
				}
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}

		public void disconnect()
		{
			if (mGoogleApiClient.IsConnected)
			{
				debugLog("Disconnecting client.");
				mGoogleApiClient.Disconnect();
			}
			else
			{
				Log.Warn(TAG, "disconnect() called when client was already disconnected.");
			}
		}

		private void giveUp(SignInFailureReason reason)
		{
			try
			{
				mConnectOnStart = false;
				disconnect();
				mSignInFailureReason = reason;

				if (reason.mActivityResultCode == GamesActivityResultCodes.ResultAppMisconfigured)
				{
					// print debug info for the developer
					GameHelperUtils.printMisconfiguredDebugInfo(mAppContext);
				}

				showFailureDialog();
				mConnecting = false;
				notifyListener(false);
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}

		/** Called when we are disconnected from the Google API client. */
		public void showFailureDialog()
		{
			if (mSignInFailureReason != null)
			{
				int errorCode = mSignInFailureReason.getServiceErrorCode();
				int actResp = mSignInFailureReason.getActivityResultCode();

				if (mShowErrorDialogs)
				{
					showFailureDialog(mActivity, actResp, errorCode);
				}
				else
				{
					debugLog("Not showing error dialog because mShowErrorDialogs==false. "
							 + "" + "Error was: " + mSignInFailureReason);
				}
			}
		}

		/** Shows an error dialog that's appropriate for the failure reason. */
		public static void showFailureDialog(Activity activity, int actResp, int errorCode)
		{
			if (activity == null)
			{
				Log.Error("GameHelper", "*** No Activity. Can't show failure dialog!");
				return;
			}
			Dialog errorDialog = null;

			switch (actResp)
			{
				case GamesActivityResultCodes.ResultAppMisconfigured:
					errorDialog = makeSimpleDialog(activity, GameHelperUtils.getString(
						activity, GameHelperUtils.R_APP_MISCONFIGURED));
					break;
				case GamesActivityResultCodes.ResultSignInFailed:
					errorDialog = makeSimpleDialog(activity, GameHelperUtils.getString(
						activity, GameHelperUtils.R_SIGN_IN_FAILED));
					break;
				case GamesActivityResultCodes.ResultLicenseFailed:
					errorDialog = makeSimpleDialog(activity, GameHelperUtils.getString(
						activity, GameHelperUtils.R_LICENSE_FAILED));
					break;
				default:
					errorDialog = GoogleApiAvailability.Instance.GetErrorDialog(activity, errorCode, RC_UNUSED);
					if (errorDialog == null)
					{
						// get fallback dialog
						Log.Error("GameHelper", "No standard error dialog available. Making fallback dialog.");
						errorDialog = makeSimpleDialog(activity,
							GameHelperUtils.getString(activity, GameHelperUtils.R_UNKNOWN_ERROR) + " " +
							GameHelperUtils.errorCodeToString(errorCode));
					}
					break;
			}

			errorDialog.Show();
		}

		public static Dialog makeSimpleDialog(Activity activity, String text)
		{
			return
				(new AlertDialog.Builder(activity)).SetMessage(text)
					.SetNeutralButton(global::Android.Resource.String.Ok, (null as EventHandler<DialogClickEventArgs>))
					.Create();
		}

		public static Dialog makeSimpleDialog(Activity activity, String title, String text)
		{
			return
				(new AlertDialog.Builder(activity)).SetMessage(text)
					.SetTitle(title)
					.SetNeutralButton(global::Android.Resource.String.Ok, (null as EventHandler<DialogClickEventArgs>))
					.Create();
		}

		public Dialog makeSimpleDialog(String text)
		{
			if (mActivity == null)
			{
				logError("*** makeSimpleDialog failed: no current Activity!");
				return null;
			}
			return makeSimpleDialog(mActivity, text);
		}

		public Dialog makeSimpleDialog(String title, String text)
		{
			if (mActivity == null)
			{
				logError("*** makeSimpleDialog failed: no current Activity!");
				return null;
			}
			return makeSimpleDialog(mActivity, title, text);
		}

		private void debugLog(String message)
		{
			if (mDebugLog)
			{
				Log.Debug(TAG, "GameHelper: " + message);
			}
		}

		private void logWarn(String message)
		{
			Log.Warn(TAG, "!!! GameHelper WARNING: " + message);
		}

		private void logError(String message)
		{
			Log.Error(TAG, "*** GameHelper ERROR: " + message);
		}
	}
}
