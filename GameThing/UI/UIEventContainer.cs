using System.Collections.Generic;
using System.Linq;
using GameThing.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameThing.UI
{
	public class UIEventContainer
	{
		public List<UIComponent> Components { get; set; } = new List<UIComponent>();

		public void InvokeContainerTap(GestureSample gesture)
		{
			var tappedComponent = Components.FirstOrDefault(component => component.IsVisible && component.IsAtPoint(gesture.Position));
			tappedComponent?.Tapped?.Invoke(gesture);
		}

		public virtual void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			Components.ForEach(component => component.LoadContent(content, graphicsDevice));
		}
	}
}
