using System;
using System.Linq;
using GameThing.UI.Config;

namespace GameThing.UI
{
	public abstract class BaseScreen
	{
		public BaseScreen(Func<ScreenComponent> load)
		{
			Screen = load();
			LoadEvents(Screen);
		}

		public UIComponent FindComponent(string id)
		{
			return FindComponent(id, Screen);
		}

		private UIComponent FindComponent(string id, UIContainer container)
		{
			foreach (var component in container.Components)
			{
				if (component is UIContainer childContainer)
				{
					var found = FindComponent(id, childContainer);
					if (found != null)
						return found;
				}

				if (component.Id == id)
					return component;
			}

			return null;
		}

		protected ScreenComponent Screen { get; private set; }

		private void LoadEvents(UIContainer container)
		{
			foreach (var component in container.Components)
			{
				if (component is UIContainer childContainer)
					LoadEvents(childContainer);

				var xmlEventProperties = component
					.GetType()
					.GetProperties()
					.Where(property => property.GetCustomAttributes(typeof(XmlEventAttribute), true).Any() && property.GetValue(component) != null)
					.ToDictionary(
						property => (string) property.GetValue(component),  // Key is name of method on parent, like "StartScreen_Tapped"
						property => property.Name                           // Value is name of property like "Tapped"
					);

				foreach (var pair in xmlEventProperties)
				{
					var eventHandlerOnParent = GetType().GetMethod(pair.Key);
					var eventOnChild = component.GetType().GetProperty($"On{pair.Value}");
					var eventHandlerOnParentDelegate = Delegate.CreateDelegate(eventOnChild.PropertyType, this, eventHandlerOnParent);

					eventOnChild.SetValue(component, eventHandlerOnParentDelegate);
				}
			}
		}
	}
}
