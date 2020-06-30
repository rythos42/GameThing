using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class StaticPanel : UIContainer
	{
		public StaticPanel()
		{
			AutoDrawChildren = true;
		}

		public Texture2D Background { get; set; }
	}
}
