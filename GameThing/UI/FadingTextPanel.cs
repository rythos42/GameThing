﻿using System;
using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class FadingTextPanel : Panel
	{
		private SpriteFont font;

		private bool showing = false;
		private bool startShowing = false;
		private TimeSpan startedShowingAt;
		private readonly Text textUi = new Text();

		public FadingTextPanel()
		{
			Components.Add(textUi);
		}

		public TimeSpan ShowFor { get; set; } = new TimeSpan(0, 0, 10);

		public void Show(string text)
		{
			textUi.Value = text;
			startShowing = true;
		}

		protected override void LoadComponentContent(Content content, GraphicsDevice graphicsDevice)
		{
			base.LoadComponentContent(content, graphicsDevice);

			font = content.Font;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (startShowing)
			{
				startedShowingAt = gameTime.TotalGameTime;
				startShowing = false;
				showing = true;
			}

			if (showing && gameTime.TotalGameTime - startedShowingAt >= ShowFor)
			{
				showing = false;
			}
		}

		public override void Draw(SpriteBatch spriteBatch, float x, float y)
		{
			if (!showing)
				return;

			base.Draw(spriteBatch, x, y);
		}
	}
}
