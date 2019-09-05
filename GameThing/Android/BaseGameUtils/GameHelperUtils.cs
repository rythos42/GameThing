﻿using System;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Games;
using Android.Util;
using Java.Security;

namespace GameThing.Android.BaseGameUtils
{
	public class GameHelperUtils
	{
		public const int R_UNKNOWN_ERROR = 0;
		public const int R_SIGN_IN_FAILED = 1;
		public const int R_APP_MISCONFIGURED = 2;
		public const int R_LICENSE_FAILED = 3;

		private static readonly string[] fallbackStrings =
		{
			"*Unknown error.",
			"*Failed to sign in. Please check your network connection and try again.",
			"*The application is incorrectly configured. Check that the package name and signing certificate match the client ID created in Developer Console. Also, if the application is not yet published, check that the account you are trying to sign in with is listed as a tester account. See logs for more information.",
			"*License check failed."
		};

		private static readonly int[] resourceIds =
		{
			Resource.String.gamehelper_unknown_error, Resource.String.gamehelper_sign_in_failed,
			Resource.String.gamehelper_app_misconfigured, Resource.String.gamehelper_license_failed
		};

		public static string ActivityResponseCodeToString(int respCode)
		{
			switch (respCode)
			{
				case (int) Result.Ok:
					return "RESULT_OK";
				case (int) Result.Canceled:
					return "RESULT_CANCELED";
				case GamesActivityResultCodes.ResultAppMisconfigured:
					return "RESULT_APP_MISCONFIGURED";
				case GamesActivityResultCodes.ResultLeftRoom:
					return "RESULT_LEFT_ROOM";
				case GamesActivityResultCodes.ResultLicenseFailed:
					return "RESULT_LICENSE_FAILED";
				case GamesActivityResultCodes.ResultReconnectRequired:
					return "RESULT_RECONNECT_REQUIRED";
				case GamesActivityResultCodes.ResultSignInFailed:
					return "SIGN_IN_FAILED";
				default:
					return Java.Lang.String.ValueOf(respCode);
			}
		}

		public static string ErrorCodeToString(int errorCode)
		{
			switch (errorCode)
			{
				case ConnectionResult.DeveloperError:
					return "DEVELOPER_ERROR(" + errorCode + ")";
				case ConnectionResult.InternalError:
					return "INTERNAL_ERROR(" + errorCode + ")";
				case ConnectionResult.InvalidAccount:
					return "INVALID_ACCOUNT(" + errorCode + ")";
				case ConnectionResult.LicenseCheckFailed:
					return "LICENSE_CHECK_FAILED(" + errorCode + ")";
				case ConnectionResult.NetworkError:
					return "NETWORK_ERROR(" + errorCode + ")";
				case ConnectionResult.ResolutionRequired:
					return "RESOLUTION_REQUIRED(" + errorCode + ")";
				case ConnectionResult.ServiceDisabled:
					return "SERVICE_DISABLED(" + errorCode + ")";
				case ConnectionResult.ServiceInvalid:
					return "SERVICE_INVALID(" + errorCode + ")";
				case ConnectionResult.ServiceMissing:
					return "SERVICE_MISSING(" + errorCode + ")";
				case ConnectionResult.ServiceVersionUpdateRequired:
					return "SERVICE_VERSION_UPDATE_REQUIRED(" + errorCode + ")";
				case ConnectionResult.SignInRequired:
					return "SIGN_IN_REQUIRED(" + errorCode + ")";
				case ConnectionResult.Success:
					return "SUCCESS(" + errorCode + ")";
				default:
					return "Unknown error code " + errorCode;
			}
		}

		public static void PrintMisconfiguredDebugInfo(Context ctx)
		{
			Log.Warn("GameHelper", "****");
			Log.Warn("GameHelper", "****");
			Log.Warn("GameHelper", "**** APP NOT CORRECTLY CONFIGURED TO USE GOOGLE PLAY GAME SERVICES");
			Log.Warn("GameHelper", "**** This is usually caused by one of these reasons:");
			Log.Warn("GameHelper", "**** (1) Your package name and certificate fingerprint do not match");
			Log.Warn("GameHelper", "****     the client ID you registered in Developer Console.");
			Log.Warn("GameHelper", "**** (2) Your App ID was incorrectly entered.");
			Log.Warn("GameHelper", "**** (3) Your game settings have not been published and you are ");
			Log.Warn("GameHelper", "****     trying to log in with an account that is not listed as");
			Log.Warn("GameHelper", "****     a test account.");
			Log.Warn("GameHelper", "****");
			if (ctx == null)
			{
				Log.Warn("GameHelper", "*** (no Context, so can't print more debug info)");
				return;
			}

			Log.Warn("GameHelper", "**** To help you debug, here is the information about this app");
			Log.Warn("GameHelper", "**** Package name         : " + ctx.PackageName);
			Log.Warn("GameHelper", "**** Cert SHA1 fingerprint: " + GetSHA1CertFingerprint(ctx));
			Log.Warn("GameHelper", "**** App ID from          : " + GetAppIdFromResource(ctx));
			Log.Warn("GameHelper", "****");
			Log.Warn("GameHelper", "**** Check that the above information matches your setup in ");
			Log.Warn("GameHelper", "**** Developer Console. Also, check that you're logging in with the");
			Log.Warn("GameHelper", "**** right account (it should be listed in the Testers section if");
			Log.Warn("GameHelper", "**** your project is not yet published).");
			Log.Warn("GameHelper", "****");
			Log.Warn("GameHelper", "**** For more information, refer to the troubleshooting guide:");
			Log.Warn("GameHelper", "****   http://developers.google.com/games/services/android/troubleshooting");
		}

		public static string GetAppIdFromResource(Context ctx)
		{
			try
			{
				var res = ctx.Resources;
				var pkgName = ctx.PackageName;
				var res_id = res.GetIdentifier("app_id", "string", pkgName);
				return res.GetString(res_id);
			}
			catch (Exception)
			{
				return "??? (failed to retrieve APP ID)";
			}
		}

		public static string GetSHA1CertFingerprint(Context ctx)
		{
			try
			{
				var sigs =
					ctx.PackageManager.GetPackageInfo(ctx.PackageName, PackageInfoFlags.Signatures).Signatures.ToArray();
				if (sigs.Length == 0)
				{
					return "ERROR: NO SIGNATURE.";
				}
				if (sigs.Length > 1)
				{
					return "ERROR: MULTIPLE SIGNATURES";
				}
				var digest = MessageDigest.GetInstance("SHA1").Digest(sigs[0].ToByteArray());
				var hexString = new StringBuilder();
				for (var i = 0; i < digest.Length; ++i)
				{
					if (i > 0)
					{
						hexString.Append(":");
					}
					ByteToString(hexString, digest[i]);
				}
				return hexString.ToString();
			}
			catch (PackageManager.NameNotFoundException ex)
			{
				ex.PrintStackTrace();
				return "(ERROR: package not found)";
			}
			catch (NoSuchAlgorithmException ex)
			{
				ex.PrintStackTrace();
				return "(ERROR: SHA1 algorithm not found)";
			}
		}

		public static void ByteToString(StringBuilder sb, byte b)
		{
			var unsigned_byte = b < 0 ? b + 256 : b;
			var hi = unsigned_byte / 16;
			var lo = unsigned_byte % 16;
			sb.Append("0123456789ABCDEF".Substring(hi, 1));
			sb.Append("0123456789ABCDEF".Substring(lo, 1));
		}

		public static string GetString(Context ctx, int whichString)
		{
			whichString = whichString >= 0 && whichString < resourceIds.Length ? whichString : 0;
			var resId = resourceIds[whichString];
			try
			{
				return ctx.GetString(resId);
			}
			catch (Exception)
			{
				Log.Warn(GameHelper.TAG, "*** GameHelper could not found resource id #" + resId + ". " +
										 "This probably happened because you included it as a stand-alone JAR. " +
										 "BaseGameUtils should be compiled as a LIBRARY PROJECT, so that it can access " +
										 "its resources. Using a fallback string.");
				return fallbackStrings[whichString];
			}
		}
	}
}
