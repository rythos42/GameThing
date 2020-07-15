using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Text : UIComponent
	{
		private SpriteFont font;
		private string value;

		[XmlText]
		public string Value
		{
			get => value;
			set
			{
				this.value = value;
				SetDimensions();
			}
		}

		private void SetDimensions()
		{
			Dimensions = Value == null || font == null ? Vector2.Zero : font.MeasureString(Value);
		}

		protected override void LoadComponentContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
		{
			font = contentManager.Load<SpriteFont>("fonts/Carlito-Regular");
			SetDimensions();
		}

		protected override void DrawComponent(SpriteBatch spriteBatch)
		{
			if (Value == null)
				return;

			spriteBatch.DrawString(font, Value, new Vector2(X, Y), Color.Black);
		}
	}
}
