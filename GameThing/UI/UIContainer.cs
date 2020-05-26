using System.Collections.Generic;
using System.Linq;
using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameThing.UI
{
	public abstract class UIContainer : UIComponent
	{
		public List<UIComponent> Components { get; set; } = new List<UIComponent>();

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
			Components.ForEach(component => component.LoadContent(content, graphicsDevice));
		}
	}
}
