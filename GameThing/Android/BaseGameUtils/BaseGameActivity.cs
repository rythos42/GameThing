using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Games.MultiPlayer.TurnBased;
using Android.OS;

namespace GameThing.Android.BaseGameUtils
{
	[Activity(Label = "BaseGameActivity")]
	public abstract class BaseGameActivity : Microsoft.Xna.Framework.AndroidGameActivity
	{
		protected static GameHelper Helper;

		protected BaseGameActivity()
		{
		}

		protected override void OnCreate(Bundle b)
		{
			base.OnCreate(b);
			if (Helper == null)
				Helper = new GameHelper(this);
		}

		protected override void OnStart()
		{
			base.OnStart();
			Helper.OnStart(this);
		}

		protected override void OnStop()
		{
			base.OnStop();
			Helper.OnStop();
		}

		protected override void OnActivityResult(int request, Result response, Intent data)
		{
			base.OnActivityResult(request, response, data);
			Helper.OnActivityResult(request, (int) response, data);
		}

		protected GoogleApiClient GetApiClient()
		{
			return Helper.GetApiClient();
		}

		protected void BeginUserInitiatedSignIn()
		{
			Helper.BeginUserInitiatedSignIn();
		}

		protected void SetTurnBasedMatch(ITurnBasedMatch turnBasedMatch)
		{
			Helper.SetTurnBasedMatch(turnBasedMatch);
		}

		protected ITurnBasedMatch GetTurnBasedMatch()
		{
			return Helper.GetTurnBasedMatch();
		}
	}
}
