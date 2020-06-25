using System;
using System.Collections;
using System.Collections.Generic;

namespace GameThing.UI
{
	public class ComponentList : IList<UIComponent>
	{
		private readonly List<UIComponent> components = new List<UIComponent>();

		public UIComponent this[int index] { get => components[index]; set => components[index] = value; }

		public int Count => components.Count;

		public bool IsReadOnly => false;

		public void Add(UIComponent item)
		{
			components.Add(item);
		}

		public void Clear()
		{
			components.Clear();
		}

		public bool Contains(UIComponent item)
		{
			return components.Contains(item);
		}

		public void CopyTo(UIComponent[] array, int arrayIndex)
		{
			components.CopyTo(array, arrayIndex);
		}

		public IEnumerator<UIComponent> GetEnumerator()
		{
			return components.GetEnumerator();
		}

		public int IndexOf(UIComponent item)
		{
			return components.IndexOf(item);
		}

		public void Insert(int index, UIComponent item)
		{
			components.Insert(index, item);
		}

		public bool Remove(UIComponent item)
		{
			return components.Remove(item);
		}

		public void RemoveAt(int index)
		{
			components.RemoveAt(index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return components.GetEnumerator();
		}

		public void ForEach(Action<UIComponent> action)
		{
			foreach (var component in components)
				action(component);
		}
	}
}
