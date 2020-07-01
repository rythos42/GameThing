using System.IO;
using System.Reflection;
using GameThing.Screens;
using Newtonsoft.Json;

namespace GameThing
{
	public static class ApplicationData
	{
		private static Configuration config;

		public static string FirebaseUrl => config.FirebaseUrl;
		public static bool AllowMergingHotSeat => config.AllowMergingHotSeat;
		public static string PlayerId => global::Android.Provider.Settings.Secure.GetString(global::Android.App.Application.Context.ContentResolver, global::Android.Provider.Settings.Secure.AndroidId);
		public static ScreenType CurrentScreen { get; set; } = ScreenType.Start;

		public static void LoadConfiguration()
		{
			var assembly = Assembly.GetAssembly(typeof(Configuration));
			var embeddedResourceStream = assembly.GetManifestResourceStream("GameThing.appsettings.json");
			if (embeddedResourceStream == null)
				return;

			using (var streamReader = new StreamReader(embeddedResourceStream))
			{
				var jsonString = streamReader.ReadToEnd();
				config = JsonConvert.DeserializeObject<Configuration>(jsonString);
			}
		}

		private class Configuration
		{
			public string FirebaseUrl { get; set; }
			public bool AllowMergingHotSeat { get; set; }
		}
	}
}
