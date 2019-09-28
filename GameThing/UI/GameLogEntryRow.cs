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

		public override void Draw(SpriteBatch spriteBatch, int x, int y)
		{
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
		}

		private Texture2D GetIconForSide(CharacterSide side)
		{
			return side == CharacterSide.Spaghetti ? spaghettiHatIcon : unicornHatIcon;
		}

		public override void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			spaghettiHatIcon = content.SpaghettiHatIcon;
			unicornHatIcon = content.UnicornHatIcon;
			cardIcon = content.CardIcon;
			moveIcon = content.MoveIcon;
		}

		public override Vector2 MeasureContent()
		{
			return new Vector2(spaghettiHatIcon.Width + cardIcon.Width + unicornHatIcon.Width, spaghettiHatIcon.Height);
		}
	}
}
