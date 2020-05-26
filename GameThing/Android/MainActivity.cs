using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using GameThing.Data;
using GameThing.Manager;
using Microsoft.Xna.Framework;

namespace GameThing.Android
{
	[Activity(Label = "GameThing", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/AppTheme", AlwaysRetainTaskState = true, LaunchMode = LaunchMode.SingleInstance,
		ScreenOrientation = ScreenOrientation.Landscape, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
	public class MainActivity : AndroidGameActivity
	{
		private MainGame game;
		private const string ChannelId = "GameThing_Channel";

		protected override void OnCreate(Bundle bundle)
		{
			Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
			Window.AddFlags(WindowManagerFlags.LayoutInOverscan);
			Window.AddFlags(WindowManagerFlags.Fullscreen);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);
			Window.AddFlags(WindowManagerFlags.TurnScreenOn);

			var uiOptions = (int) Window.DecorView.SystemUiVisibility;
			uiOptions |= (int) SystemUiFlags.LowProfile;
			uiOptions |= (int) SystemUiFlags.Fullscreen;
			uiOptions |= (int) SystemUiFlags.HideNavigation;
			uiOptions |= (int) SystemUiFlags.ImmersiveSticky;
			Window.DecorView.SystemUiVisibility = (StatusBarVisibility) uiOptions;

			base.OnCreate(bundle);

			ApplicationData.LoadConfiguration();
			game = new MainGame();
			SetContentView((View) game.Services.GetService(typeof(View)));
			BattleManager.Instance.DataUpdated += BattleManager_DataUpdated;
			CreateNotificationChannel();

			game.Run();
		}

		private void BattleManager_DataUpdated(BattleData battleData)
		{
			ShowNotification();
		}

		private void CreateNotificationChannel()
		{
			if (Build.VERSION.SdkInt < BuildVersionCodes.O)
				return;

			var channelName = Resources.GetString(Resource.String.ApplicationName);
			var channel = new NotificationChannel(ChannelId, channelName, NotificationImportance.Default);

			var notificationManager = (NotificationManager) GetSystemService(NotificationService);
			notificationManager.CreateNotificationChannel(channel);
		}

		private void ShowNotification()
		{
			var intent = new Intent(this, typeof(MainActivity));

			const int pendingIntentId = 0;
			var pendingIntent = PendingIntent.GetActivity(this, pendingIntentId, intent, PendingIntentFlags.OneShot);

			var notification = new NotificationCompat.Builder(this, ChannelId)
				.SetContentIntent(pendingIntent)
				.SetContentTitle("GameThing battle update!")
				.SetContentText("One of your games has been updated.")
				.SetSmallIcon(Resource.Drawable.Icon)
				.Build();

			var notificationManager = GetSystemService(NotificationService) as NotificationManager;
			const int notificationId = 0;
			notificationManager.Notify(notificationId, notification);
		}
	}
}
