using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Screens
{
	public class GameOverScreen
	{
		private SpriteFont font;

		public CharacterSide? Winner { get; set; }

		public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
		{
			font = content.Load<SpriteFont>("fonts/Carlito-Regular");
		}

		public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			graphicsDevice.Clear(Color.White);

			spriteBatch.Begin();
			spriteBatch.DrawString(font, "GAME OVER, WINNER IS", new Vector2(300, 300), Color.Black);

			var winner = Winner == null ? "NO ONE" : Winner.ToString();
			spriteBatch.DrawString(font, winner, new Vector2(300, 500), Color.Black);
			spriteBatch.End();
		}
	}
}
