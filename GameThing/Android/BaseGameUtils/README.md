Copied from
	https://github.com/slown1/Xamarin.PlayServices.BaseGameUtils

Commented out the Plus, Drive and AppState features as I don't currently use them and didn't want to NuGet those packages.
Commented out Requests, because they are a deprecated feature of Google Play Games Services.
	https://android-developers.googleblog.com/2017/04/focusing-our-google-play-games-services.html
Made it inherit from Microsoft.Xna.Framework.AndroidGameActivity
Added setTurnBasedMatch so subclass of BaseGameActivity could use it to set a common location for it after connecting