using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class Button : UIComponent
	{
		private Texture2D buttonLeft;
		private Texture2D buttonRight;
		private Texture2D buttonTopBottom;
		private Texture2D buttonLeftDisabled;
		private Texture2D buttonRightDisabled;
		private Texture2D buttonTopBottomDisabled;

		private SpriteFont font;
		private string text;
		protected const int MINIMUM_BUTTON_WIDTH = 300;

		public Button()
		{
			Padding = 16;
		}

		[XmlIgnore]
		public bool IsHighlighted { get; set; }

		[XmlAttribute]
		public bool UseMinimumButtonSize { get; set; } = true;

		[XmlText]
		public string Text
		{
			get => text;
			set
			{
				text = value;

				void updateUiFromText()
				{
					var textSize = font.MeasureString(text);
					var minimumTextWidth = (int) textSize.X + (Padding * 2);
					Width = UseMinimumButtonSize ? MathHelper.Max(minimumTextWidth, MINIMUM_BUTTON_WIDTH) : minimumTextWidth;
					Height = (int) textSize.Y + (Padding * 2);

					// remove self from event
					OnContentLoaded -= updateUiFromText;
				}

				// If we haven't loaded content yet, set an event to update after we do so.
				if (!HasContentLoaded)
					OnContentLoaded += updateUiFromText;
				else
					updateUiFromText();
			}
		}

		protected override void LoadComponentContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
		{
			buttonLeft = contentManager.Load<Texture2D>("sprites/ui/button_left"); ;
			buttonRight = contentManager.Load<Texture2D>("sprites/ui/button_right");
			buttonTopBottom = contentManager.Load<Texture2D>("sprites/ui/button_top_bottom");
			buttonLeftDisabled = contentManager.Load<Texture2D>("sprites/ui/button_left_disabled");
			buttonRightDisabled = contentManager.Load<Texture2D>("sprites/ui/button_right_disabled");
			buttonTopBottomDisabled = contentManager.Load<Texture2D>("sprites/ui/button_top_bottom_disabled");

			font = contentManager.Load<SpriteFont>("fonts/PatrickHandSC-Regular");
		}

		protected override void DrawComponent(SpriteBatch spriteBatch)
		{
			var spacingCount = Width - buttonLeft.Width - buttonRight.Width;
			for (var i = 0; i < spacingCount; i++)
			{
				spriteBatch.Draw(Enabled ? buttonTopBottom : buttonTopBottomDisabled, new Rectangle((int) X + buttonLeft.Width + i, (int) Y, buttonTopBottom.Width, buttonTopBottom.Height), Color.White);
				spriteBatch.Draw(Enabled ? buttonTopBottom : buttonTopBottomDisabled, new Rectangle((int) X + buttonLeft.Width + i, (int) Y + Height - buttonTopBottom.Height, buttonTopBottom.Width, buttonTopBottom.Height), Color.White);
			}

			spriteBatch.Draw(Enabled ? buttonLeft : buttonLeftDisabled, new Rectangle((int) X, (int) Y, buttonLeft.Width, Height), Color.White);
			spriteBatch.Draw(Enabled ? buttonRight : buttonRightDisabled, new Rectangle((int) X + Width - buttonRight.Width, (int) Y, buttonRight.Width, Height), Color.White);

			spriteBatch.DrawString(font, Text, new Vector2(X + Padding, Y + Padding), Enabled ? Color.Black : new Color(140, 140, 140));
		}
	}
}
