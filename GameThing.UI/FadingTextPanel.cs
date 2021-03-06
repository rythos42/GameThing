﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class FadingTextPanel : Panel
	{
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

		protected override void DrawComponent(SpriteBatch spriteBatch)
		{
			if (!showing)
				return;

			base.DrawComponent(spriteBatch);
		}
	}
}
