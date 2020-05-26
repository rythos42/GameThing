using GameThing.Data;
using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class GameLogEntryRow : UIComponent
	{
		private Texture2D spaghettiHatIcon;
		private Texture2D unicornHatIcon;
		private Texture2D moveIcon;
		private Texture2D cardIcon;

		public GameLogEntry GameLogEntry { get; set; }

		public override void Draw(SpriteBatch spriteBatch, float x, float y)
		{
			X = x;
			Y = y;

			if (GameLogEntry == null)
				return;

			var sourceIcon = GetIconForSide(GameLogEntry.SourceCharacterSide);
			spriteBatch.Draw(sourceIcon, new Vector2(x, y), Color.White);

			if (GameLogEntry.MovedTo != null)
			{
				spriteBatch.Draw(moveIcon, new Vector2(x + sourceIcon.Width, y), Color.White);
			}
			else
			{
				spriteBatch.Draw(cardIcon, new Vector2(x + sourceIcon.Width, y), Color.White);

				var targetIcon = GetIconForSide(GameLogEntry.TargetCharacterSide);
				spriteBatch.Draw(targetIcon, new Vector2(x + sourceIcon.Width + cardIcon.Width, y), Color.White);
			}
			IsVisible = true;
		}

		private Texture2D GetIconForSide(CharacterSide side)
		{
			return side == CharacterSide.Spaghetti ? spaghettiHatIcon : unicornHatIcon;
		}

		protected override void LoadComponentContent(Content content, GraphicsDevice graphicsDevice)
		{
			spaghettiHatIcon = content.SpaghettiHatIcon;
			unicornHatIcon = content.UnicornHatIcon;
			cardIcon = content.CardIcon;
			moveIcon = content.MoveIcon;

			Width = spaghettiHatIcon.Width + cardIcon.Width + unicornHatIcon.Width;
			Height = spaghettiHatIcon.Height;
		}

		public override Vector2 MeasureContent()
		{
			return new Vector2(Width, Height);
		}
	}
}
