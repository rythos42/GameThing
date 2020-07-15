using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Image : UIComponent
	{
		private Texture2D texture;

		[XmlIgnore]
		public Texture2D Texture
		{
			get => texture;
			set
			{
				texture = value;
				Width = value.Width;
				Height = value.Height;
			}
		}

		protected override void DrawComponent(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Texture, new Rectangle((int) X, (int) Y, Width, Height), Color.White);
		}

		protected override void LoadComponentContent(ContentManager content, GraphicsDevice graphicsDevice)
		{
		}
	}
}
