namespace GameThing.Android.BaseGameUtils
{
	public class SignInFailureReason
	{
		public const int NO_ACTIVITY_RESULT_CODE = -100;
		private readonly int mServiceErrorCode;

		public SignInFailureReason(int serviceErrorCode, int activityResultCode)
		{
			mServiceErrorCode = serviceErrorCode;
			ActivityResultCode = activityResultCode;
		}

		public SignInFailureReason(int serviceErrorCode) : this(serviceErrorCode, NO_ACTIVITY_RESULT_CODE)
		{
		}

		public int GetServiceErrorCode()
		{
			return mServiceErrorCode;
		}

		public int ActivityResultCode { get; set; } = NO_ACTIVITY_RESULT_CODE;

		public override string ToString()
		{
			return "SignInFailureReason(serviceErrorCode:"
				   + GameHelperUtils.ErrorCodeToString(mServiceErrorCode)
				   + ((ActivityResultCode == NO_ACTIVITY_RESULT_CODE)
					   ? ")"
					   : (",activityResultCode:" + GameHelperUtils.ActivityResponseCodeToString(ActivityResultCode) + ")"));
		}
	}
}
