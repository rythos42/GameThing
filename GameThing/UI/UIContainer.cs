using System.Linq;
using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameThing.UI
{
	public abstract class UIContainer : UIComponent
	{
		private Texture2D borderTexture;

		public ComponentList Components { get; set; } = new ComponentList();
		public bool AutoDrawChildren { get; set; } = false;
		public bool ExtendedPadding { get; set; } = false;
		public bool ShowBorder { get; set; } = false;

		protected UIComponent GetComponentAt(Vector2 position)
		{
			return Components.FirstOrDefault(component => component.IsVisible && component.IsAtPoint(position));
		}

		public void InvokeContainerTap(GestureSample gesture)
		{
			var component = GetComponentAt(gesture.Position);
			if (component?.Enabled == true)
				component?.Tapped?.Invoke(component?.Id, gesture);

			var componentAsContainer = component as UIContainer;
			componentAsContainer?.InvokeContainerTap(gesture);
		}

		public void InvokeContainerHeld(GestureSample gesture)
		{
			var component = GetComponentAt(gesture.Position);
			if (component?.Enabled == true)
				component?.Held?.Invoke(component, gesture);

			var componentAsContainer = component as UIContainer;
			componentAsContainer?.InvokeContainerHeld(gesture);
		}

		public void InvokeContainerGestureRead(GestureSample gesture)
		{
			GestureRead?.Invoke(gesture);
		}

		protected override void LoadComponentContent(Content content, GraphicsDevice graphicsDevice)
		{
			// Store these for when we add components later
			Components.Content = content;
			Components.GraphicsDevice = graphicsDevice;
			Components.ForEach(component =>
			{
				if (!component.HasContentLoaded)
					component.LoadContent(content, graphicsDevice);
			});

			borderTexture = content.BlackTexture;
		}

		public override void Update(GameTime gameTime)
		{
			Components.ForEach(component => component.Update(gameTime));
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!AutoDrawChildren)
				return;

			if (ShowBorder)
			{
				var borderWidth = 2;
				spriteBatch.Draw(borderTexture, new Rectangle((int) X, (int) Y, Width, borderWidth), Color.White);            // Top
				spriteBatch.Draw(borderTexture, new Rectangle((int) X, (int) Y + Height, Width, borderWidth), Color.White);   // Bottom
				spriteBatch.Draw(borderTexture, new Rectangle((int) X, (int) Y, borderWidth, Height), Color.White);           // Left
				spriteBatch.Draw(borderTexture, new Rectangle((int) X + Width, (int) Y, borderWidth, Height), Color.White);   // Right
			}

			var drawX = X + MARGIN + (ExtendedPadding ? 32 : 0);
			var drawY = Y + MARGIN + (ExtendedPadding ? 32 : 0);
			Components.ForEach(component =>
			{
				component.X = drawX;
				component.Y = drawY;
				component.Draw(spriteBatch);
				drawY += MARGIN + component.Height;
			});
			IsVisible = true;
		}
	}
}
