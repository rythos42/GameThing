using GameThing.Entities;
using Microsoft.Xna.Framework;
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

		public Button(string text)
		{
			Text = text;
			Padding = 16;
		}

		public bool IsHighlighted { get; set; }

		public bool UseMinimumButtonSize { get; set; } = true;

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
					ContentLoaded -= updateUiFromText;
				}

				// If we haven't loaded content yet, set an event to update after we do so.
				if (!HasContentLoaded)
					ContentLoaded += updateUiFromText;
				else
					updateUiFromText();
			}
		}

		protected override void LoadComponentContent(Content content, GraphicsDevice graphicsDevice)
		{
			buttonLeft = content.ButtonLeft;
			buttonRight = content.ButtonRight;
			buttonTopBottom = content.ButtonTopBottom;
			buttonLeftDisabled = content.ButtonLeftDisabled;
			buttonRightDisabled = content.ButtonRightDisabled;
			buttonTopBottomDisabled = content.ButtonTopBottomDisabled;

			font = content.PatrickHandSc;
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
