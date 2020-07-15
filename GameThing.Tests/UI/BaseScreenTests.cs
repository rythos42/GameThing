using GameThing.UI;
using GameThing.UI.Config;
using Microsoft.Xna.Framework.Input.Touch;
using NUnit.Framework;

namespace GameThing.Tests.UI.Config
{
	[TestFixture]
	public class BaseScreenTests
	{
		[Test]
		public void BaseScreen_LoadsXml_AttachesEvents()
		{
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ScreenComponent xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<UIComponent xsi:type=""Panel"">
		<UIComponent xsi:type=""Button"" Tapped=""StartHotSeat_Tapped"" Id=""HotSeat"">Hot Seat</UIComponent>
	</UIComponent>
</ScreenComponent>
			";

			var screen = new TestScreen(xml);
			var button = screen.FindComponent("HotSeat");

			Assert.That(button, Is.Not.Null);

			button.OnTapped.Invoke("Id", new GestureSample());

			Assert.That(screen.StartHotSeatEventTriggered, Is.True);
		}

		public class TestScreen : BaseScreen
		{
			public TestScreen(string xml) : base(() => new StringConfigurationLoader().Load(xml)) { }

			public bool StartHotSeatEventTriggered { get; set; }

			public void StartHotSeat_Tapped(string id, GestureSample gesture)
			{
				StartHotSeatEventTriggered = true;
			}
		}
	}
}
