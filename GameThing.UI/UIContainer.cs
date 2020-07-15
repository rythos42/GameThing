using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameThing.UI
{
	public abstract class UIContainer : UIComponent
	{
		private Texture2D borderTexture;

		[XmlElement("UIComponent")]
		public ComponentList Components { get; set; } = new ComponentList();

		[XmlAttribute]
		public bool ShowBorder { get; set; } = false;

		[XmlIgnore]
		public Texture2D Background { get; set; }

		protected UIComponent GetComponentAt(Vector2 position)
		{
			return Components.FirstOrDefault(component => component.IsVisible && component.IsAtPoint(position));
		}

		public void InvokeContainerTap(GestureSample gesture)
		{
			var component = GetComponentAt(gesture.Position);
			if (component?.Enabled == true)
				component?.OnTapped?.Invoke(component?.Id, gesture);

			var componentAsContainer = component as UIContainer;
			componentAsContainer?.InvokeContainerTap(gesture);
		}

		public void InvokeContainerHeld(GestureSample gesture)
		{
			var component = GetComponentAt(gesture.Position);
			if (component?.Enabled == true)
				component?.OnHeld?.Invoke(component, gesture);

			var componentAsContainer = component as UIContainer;
			componentAsContainer?.InvokeContainerHeld(gesture);
		}

		public void InvokeContainerGestureRead(GestureSample gesture)
		{
			OnGestureRead?.Invoke(gesture);
		}

		protected override void LoadComponentContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
		{
			// Store these for when we add components later
			Components.ContentManager = contentManager;
			Components.GraphicsDevice = graphicsDevice;
			Components.ForEach(component =>
			{
				if (!component.HasContentLoaded)
					component.LoadContent(contentManager, graphicsDevice);
			});

			borderTexture = new Texture2D(graphicsDevice, 1, 1);
			borderTexture.SetData(new Color[] { Color.Black });
		}

		public override void Update(GameTime gameTime)
		{
			Components.ForEach(component => component.Update(gameTime));
		}

		protected override void DrawComponent(SpriteBatch spriteBatch)
		{
			if (Background != null)
				spriteBatch.Draw(Background, new Rectangle((int) X, (int) Y, Width, Height), Color.White);

			if (ShowBorder)
			{
				var borderWidth = 2;
				spriteBatch.Draw(borderTexture, new Rectangle((int) X, (int) Y, Width, borderWidth), Color.White);            // Top
				spriteBatch.Draw(borderTexture, new Rectangle((int) X, (int) Y + Height, Width, borderWidth), Color.White);   // Bottom
				spriteBatch.Draw(borderTexture, new Rectangle((int) X, (int) Y, borderWidth, Height), Color.White);           // Left
				spriteBatch.Draw(borderTexture, new Rectangle((int) X + Width, (int) Y, borderWidth, Height), Color.White);   // Right
			}
		}
	}
}
