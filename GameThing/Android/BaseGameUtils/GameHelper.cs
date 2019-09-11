using System;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Drive;
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

		private const int requestCode_Resolve = 9001;              // Request code we use when invoking other Activities to complete the sign-in flow.
		private const int requestCode_Unused = 9002;               // Request code when invoking Activities whose result we don't care about.

		private readonly Handler mHandler;
		private const String gameHelperSharedPrefs = "GAMEHELPER_SHARED_PREFS";
		private const String keySignInCancellations = "KEY_SIGN_IN_CANCELLATIONS";
		private Activity mActivity;
		private Context mAppContext;
		private bool mConnectOnStart = true;
		private bool mConnecting;
		private ConnectionResult mConnectionResult;
		private readonly bool mDebugLog = true;
		private bool mExpectingResolution;
		private GoogleApiClient mGoogleApiClient;
		private IGameHelperListener mListener;
		private readonly int mMaxAutoSignInAttempts = 3;
		private bool mSetupDone;
		private readonly bool mShowErrorDialogs = true;
		private bool mSignInCancelled;
		private SignInFailureReason mSignInFailureReason;
		private ITurnBasedMatch mTurnBasedMatch;
		private bool mUserInitiatedSignIn;

		public GameHelper(Activity activity)
		{
			mActivity = activity;
			mAppContext = activity.ApplicationContext;
			mHandler = new Handler();
			Setup(activity as IGameHelperListener);
		}

		public void OnConnected(Bundle connectionHint)
		{
			DebugLog("onConnected: connected!");

			if (connectionHint != null)
			{
				DebugLog("onConnected: connection hint provided. Checking for TBMP game.");
				mTurnBasedMatch = connectionHint.GetParcelable(Multiplayer.ExtraTurnBasedMatch).JavaCast<ITurnBasedMatch>();
			}

			// we're good to go
			SucceedSignIn();
		}

		public void OnConnectionSuspended(int cause)
		{
			DebugLog("onConnectionSuspended, cause=" + cause);
			Disconnect();
			mSignInFailureReason = null;
			DebugLog("Making extraordinary call to OnSignInFailed callback");
			mConnecting = false;
			NotifyListener(false);
		}

		public void OnConnectionFailed(ConnectionResult result)
		{
			// save connection result for later reference
			DebugLog("onConnectionFailed");

			mConnectionResult = result;
			DebugLog("Connection failure:");
			DebugLog("   - code: " + GameHelperUtils.ErrorCodeToString(mConnectionResult.ErrorCode));
			DebugLog("   - resolvable: " + mConnectionResult.HasResolution);
			DebugLog("   - details: " + mConnectionResult);

			var cancellations = GetSignInCancellations();
			bool shouldResolve;
			if (mUserInitiatedSignIn)
			{
				DebugLog("onConnectionFailed: WILL resolve because user initiated sign-in.");
				shouldResolve = true;
			}
			else if (mSignInCancelled)
			{
				DebugLog("onConnectionFailed WILL NOT resolve (user already cancelled once).");
				shouldResolve = false;
			}
			else if (cancellations < mMaxAutoSignInAttempts)
			{
				DebugLog("onConnectionFailed: WILL resolve because we have below the max# of "
						 + "attempts, "
						 + cancellations
						 + " < "
						 + mMaxAutoSignInAttempts);
				shouldResolve = true;
			}
			else
			{
				shouldResolve = false;
				DebugLog("onConnectionFailed: Will NOT resolve; not user-initiated and max attempts "
						 + "reached: "
						 + cancellations
						 + " >= "
						 + mMaxAutoSignInAttempts);
			}

			if (!shouldResolve)
			{
				// Fail and wait for the user to want to sign in.
				DebugLog("onConnectionFailed: since we won't resolve, failing now.");
				mConnectionResult = result;
				mConnecting = false;
				NotifyListener(false);
				return;
			}

			DebugLog("onConnectionFailed: resolving problem...");

			// Resolve the connection result. This usually means showing a dialog or
			// starting an Activity that will allow the user to give the appropriate
			// consents so that sign-in can be successful.
			ResolveConnectionResult();
		}

		private void AssertConfigured(String operation)
		{
			if (!mSetupDone)
			{
				var error = "GameHelper error: Operation attempted without setup: "
							   + operation
							   + ". The setup() method must be called before attempting any other operation.";
				LogError(error);
				throw new IllegalStateException(error);
			}
		}

		private void Setup(IGameHelperListener listener)
		{
			if (mSetupDone)
			{
				var error = "GameHelper: you cannot call GameHelper.setup() more than once!";
				LogError(error);
				throw new IllegalStateException(error);
			}
			mListener = listener ?? throw new Exception("The listener is not of type IGameHelperListener");

			using (var builder = new GoogleApiClient.Builder(mActivity, this, this))
			{
				mGoogleApiClient = builder
									.AddApi(GamesClass.API)
									.AddScope(GamesClass.ScopeGames)
									.AddApi(DriveClass.API)
									.AddScope(DriveClass.ScopeAppfolder)
									.Build();
			}

			mSetupDone = true;
		}

		public GoogleApiClient GetApiClient()
		{
			if (mGoogleApiClient == null)
				throw new IllegalStateException("No GoogleApiClient. Did you call setup()?");
			return mGoogleApiClient;
		}

		public void OnStart(Activity act)
		{
			try
			{
				mActivity = act;
				mAppContext = act.ApplicationContext;

				DebugLog("onStart");
				AssertConfigured("onStart");

				if (mConnectOnStart)
				{
					if (mGoogleApiClient.IsConnected)
					{
						Log.Warn(TAG,
							"GameHelper: client was already connected on onStart()");
					}
					else
					{
						DebugLog("Connecting client.");
						mConnecting = true;
						mGoogleApiClient.Connect();
					}
				}
				else
				{
					DebugLog("Not attempting to connect becase mConnectOnStart=false");
					DebugLog("Instead, reporting a sign-in failure.");
					var runnable = new Runnable(() => { NotifyListener(false); });
					mHandler.PostDelayed(runnable, 1000);
				}
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}

		public void OnStop()
		{
			DebugLog("onStop");
			AssertConfigured("onStop");
			if (mGoogleApiClient.IsConnected)
			{
				DebugLog("Disconnecting client due to onStop");
				mGoogleApiClient.Disconnect();
			}
			else
			{
				DebugLog("Client already disconnected when we got onStop.");
			}
			mConnecting = false;
			mExpectingResolution = false;

			// let go of the Activity reference
			mActivity = null;
		}

		public void SetTurnBasedMatch(ITurnBasedMatch turnBasedMatch)
		{
			mTurnBasedMatch = turnBasedMatch;
		}

		public ITurnBasedMatch GetTurnBasedMatch()
		{
			if (!mGoogleApiClient.IsConnected)
				Log.Warn(TAG, "Warning: getTurnBasedMatch() should only be called when signed in, that is, after getting onSignInSuceeded()");
			return mTurnBasedMatch;
		}

#pragma warning disable IDE0060 // Remove unused parameter
		public void OnActivityResult(int requestCode, int responseCode, Intent intent)
#pragma warning restore IDE0060 // Remove unused parameter
		{
			DebugLog("onActivityResult: req="
					 + (requestCode == requestCode_Resolve ? "RC_RESOLVE" : Java.Lang.String.ValueOf(requestCode)) +
					 ", resp="
					 + GameHelperUtils.ActivityResponseCodeToString(responseCode));
			if (requestCode != requestCode_Resolve)
			{
				DebugLog("onActivityResult: request code not meant for us. Ignoring.");
				return;
			}

			// no longer expecting a resolution
			mExpectingResolution = false;

			if (!mConnecting)
			{
				DebugLog("onActivityResult: ignoring because we are not connecting.");
				return;
			}

			// We're coming back from an activity that was launched to resolve a connection problem. For example, the sign-in UI.
			if (responseCode == (int) Result.Ok)
			{
				// Ready to try to connect again.
				DebugLog("onAR: Resolution was RESULT_OK, so connecting current client again.");
				Connect();
			}
			else if (responseCode == GamesActivityResultCodes.ResultReconnectRequired)
			{
				DebugLog("onAR: Resolution was RECONNECT_REQUIRED, so reconnecting.");
				Connect();
			}
			else if (responseCode == (int) Result.Canceled)
			{
				// User cancelled.
				DebugLog("onAR: Got a cancellation result, so disconnecting.");
				mSignInCancelled = true;
				mConnectOnStart = false;
				mUserInitiatedSignIn = false;
				mSignInFailureReason = null; // cancelling is not a failure!
				mConnecting = false;
				mGoogleApiClient.Disconnect();

				// increment # of cancellations
				var prevCancellations = GetSignInCancellations();
				var newCancellations = IncrementSignInCancellations();
				DebugLog("onAR: # of cancellations " + prevCancellations + " --> " + newCancellations + ", max " + mMaxAutoSignInAttempts);

				NotifyListener(false);
			}
			else
			{
				// Whatever the problem we were trying to solve, it was not solved. So give up and show an error message.
				DebugLog("onAR: responseCode=" + GameHelperUtils.ActivityResponseCodeToString(responseCode) + ", so giving up.");
				GiveUp(new SignInFailureReason(mConnectionResult.ErrorCode, responseCode));
			}
		}

		private void NotifyListener(bool success)
		{
			DebugLog("Notifying LISTENER of sign-in "
					 + (success ? "SUCCESS" : mSignInFailureReason != null ? "FAILURE (error)" : "FAILURE (no error)"));
			if (mListener != null)
			{
				if (success)
					mListener.OnSignInSucceeded();
				else
					mListener.OnSignInFailed();
			}
		}

		public void BeginUserInitiatedSignIn()
		{
			DebugLog("beginUserInitiatedSignIn: resetting attempt count.");
			ResetSignInCancellations();
			mSignInCancelled = false;
			mConnectOnStart = true;

			if (mGoogleApiClient.IsConnected)
			{
				// nothing to do
				LogWarn("beginUserInitiatedSignIn() called when already connected. Calling listener directly to notify of success.");
				NotifyListener(true);
				return;
			}
			if (mConnecting)
			{
				LogWarn("beginUserInitiatedSignIn() called when already connecting. "
						+ "Be patient! You can only call this method after you get an "
						+ "OnSignInSucceeded() or OnSignInFailed() callback. Suggestion: disable "
						+ "the sign-in button on startup and also when it's clicked, and re-enable "
						+ "when you get the callback.");
				// ignore call (listener will get a callback when the connection process finishes)
				return;
			}

			DebugLog("Starting USER-INITIATED sign-in flow.");

			// indicate that user is actively trying to sign in (so we know to resolve connection problems by showing dialogs)
			mUserInitiatedSignIn = true;

			if (mConnectionResult != null)
			{
				// We have a pending connection result from a previous failure, so start with that.
				DebugLog("beginUserInitiatedSignIn: continuing pending sign-in flow.");
				mConnecting = true;
				ResolveConnectionResult();
			}
			else
			{
				// We don't have a pending connection result, so start anew.
				DebugLog("beginUserInitiatedSignIn: starting new sign-in flow.");
				mConnecting = true;
				Connect();
			}
		}

		private void Connect()
		{
			if (mGoogleApiClient.IsConnected)
			{
				DebugLog("Already connected.");
				return;
			}
			DebugLog("Starting connection.");
			mConnecting = true;
			mTurnBasedMatch = null;
			mGoogleApiClient.Connect();
		}

		private void SucceedSignIn()
		{
			DebugLog("succeedSignIn");
			mSignInFailureReason = null;
			mConnectOnStart = true;
			mUserInitiatedSignIn = false;
			mConnecting = false;
			NotifyListener(true);
		}

		private int GetSignInCancellations()
		{
			var sp = mAppContext.GetSharedPreferences(gameHelperSharedPrefs, FileCreationMode.Private);
			return sp.GetInt(keySignInCancellations, 0);
		}

		private int IncrementSignInCancellations()
		{
			var cancellations = GetSignInCancellations();
			var editor = mAppContext.GetSharedPreferences(gameHelperSharedPrefs, FileCreationMode.Private).Edit();
			editor.PutInt(keySignInCancellations, cancellations + 1);
			editor.Commit();
			return cancellations + 1;
		}

		private void ResetSignInCancellations()
		{
			var editor = mAppContext.GetSharedPreferences(gameHelperSharedPrefs, FileCreationMode.Private).Edit();
			editor.PutInt(keySignInCancellations, 0);
			editor.Commit();
		}

		private void ResolveConnectionResult()
		{
			try
			{
				// Try to resolve the problem
				if (mExpectingResolution)
				{
					DebugLog("We're already expecting the result of a previous resolution.");
					return;
				}

				DebugLog("resolveConnectionResult: trying to resolve result: " + mConnectionResult);
				if (mConnectionResult.HasResolution)
				{
					// This problem can be fixed. So let's try to fix it.
					DebugLog("Result has resolution. Starting it.");
					try
					{
						// launch appropriate UI flow (which might, for example, be the sign-in flow)
						mExpectingResolution = true;
						mConnectionResult.StartResolutionForResult(mActivity, requestCode_Resolve);
					}
					catch (IntentSender.SendIntentException e)
					{
						e.ToString();
						// Try connecting again
						DebugLog("SendIntentException, so connecting again.");
						Connect();
					}
				}
				else
				{
					// It's not a problem what we can solve, so give up and show an error.
					DebugLog("resolveConnectionResult: result has no resolution. Giving up.");
					GiveUp(new SignInFailureReason(mConnectionResult.ErrorCode));
				}
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}

		public void Disconnect()
		{
			if (mGoogleApiClient.IsConnected)
			{
				DebugLog("Disconnecting client.");
				mGoogleApiClient.Disconnect();
			}
			else
			{
				Log.Warn(TAG, "disconnect() called when client was already disconnected.");
			}
		}

		private void GiveUp(SignInFailureReason reason)
		{
			mConnectOnStart = false;
			Disconnect();
			mSignInFailureReason = reason;

			if (reason.ActivityResultCode == GamesActivityResultCodes.ResultAppMisconfigured)
				GameHelperUtils.PrintMisconfiguredDebugInfo(mAppContext);

			ShowFailureDialog();
			mConnecting = false;
			NotifyListener(false);
		}

		private void ShowFailureDialog()
		{
			if (mSignInFailureReason != null)
			{
				var errorCode = mSignInFailureReason.GetServiceErrorCode();
				var actResp = mSignInFailureReason.ActivityResultCode;

				if (mShowErrorDialogs)
					ShowFailureDialog(mActivity, actResp, errorCode);
				else
					DebugLog("Not showing error dialog because mShowErrorDialogs==false. Error was: " + mSignInFailureReason);
			}
		}

		private static void ShowFailureDialog(Activity activity, int actResp, int errorCode)
		{
			if (activity == null)
			{
				Log.Error("GameHelper", "*** No Activity. Can't show failure dialog!");
				return;
			}
			Dialog errorDialog;
			switch (actResp)
			{
				case GamesActivityResultCodes.ResultAppMisconfigured:
					errorDialog = MakeSimpleDialog(activity, GameHelperUtils.GetString(
						activity, GameHelperUtils.R_APP_MISCONFIGURED));
					break;
				case GamesActivityResultCodes.ResultSignInFailed:
					errorDialog = MakeSimpleDialog(activity, GameHelperUtils.GetString(
						activity, GameHelperUtils.R_SIGN_IN_FAILED));
					break;
				case GamesActivityResultCodes.ResultLicenseFailed:
					errorDialog = MakeSimpleDialog(activity, GameHelperUtils.GetString(
						activity, GameHelperUtils.R_LICENSE_FAILED));
					break;
				default:
					errorDialog = GoogleApiAvailability.Instance.GetErrorDialog(activity, errorCode, requestCode_Unused);
					if (errorDialog == null)
					{
						// get fallback dialog
						Log.Error("GameHelper", "No standard error dialog available. Making fallback dialog.");
						errorDialog = MakeSimpleDialog(activity,
							GameHelperUtils.GetString(activity, GameHelperUtils.R_UNKNOWN_ERROR) + " " +
							GameHelperUtils.ErrorCodeToString(errorCode));
					}
					break;
			}

			errorDialog.Show();
		}

		private static Dialog MakeSimpleDialog(Activity activity, String text)
		{
			using (var builder = new AlertDialog.Builder(activity))
			{
				return builder.SetMessage(text)
						.SetNeutralButton(global::Android.Resource.String.Ok, null as EventHandler<DialogClickEventArgs>)
						.Create();
			}
		}

		private void DebugLog(String message)
		{
			if (mDebugLog)
				Log.Debug(TAG, "GameHelper: " + message);
		}

		private void LogWarn(String message)
		{
			Log.Warn(TAG, "!!! GameHelper WARNING: " + message);
		}

		private void LogError(String message)
		{
			Log.Error(TAG, "*** GameHelper ERROR: " + message);
		}
	}
}
