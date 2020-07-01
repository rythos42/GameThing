using GameThing.Contract;
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

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (GameLogEntry == null)
				return;

			var sourceIcon = GetIconForSide(GameLogEntry.SourceCharacterSide);
			var smallIconWidth = sourceIcon.Width / 2;
			var smallIconHeight = sourceIcon.Height / 2;
			var nextX = X + (smallIconWidth / 2);
			if (GameLogEntry.MovedTo != null)
			{
				spriteBatch.Draw(moveIcon, new Vector2(X + 2, Y + 10), Color.White);
			}
			else
			{
				spriteBatch.Draw(cardIcon, new Vector2(X + 2, Y + 10), Color.White);

				var targetIcon = GetIconForSide(GameLogEntry.TargetCharacterSide);
				nextX += smallIconWidth / 2;
				spriteBatch.Draw(targetIcon, new Rectangle((int) nextX, (int) (Y + (cardIcon.Height * 0.75)), smallIconWidth, smallIconHeight), Color.White);
			}

			spriteBatch.Draw(sourceIcon, new Rectangle((int) X, (int) Y, smallIconWidth, smallIconHeight), Color.White);
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

			Width = cardIcon.Width; // this is the result of how it's drawn above
			Height = spaghettiHatIcon.Height;
		}
	}
}
