using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameThing.Entities.Cards;
using Newtonsoft.Json;

namespace GameThing.Database
{
	public interface ICategoryMapper : IIdentifiableMapper<Category>
	{
		void Load();
	}

	public class CategoryMapper : ICategoryMapper
	{
		private static ICategoryMapper instance;
		private List<Category> categories;

		public static ICategoryMapper Instance
		{
			get
			{
				if (instance == null)
					instance = new CategoryMapper();
				return instance;
			}
			internal set { instance = value; }
		}

		private CategoryMapper() { }

		public void Load()
		{
			var assembly = Assembly.GetAssembly(typeof(CategoryMapper));
			var embeddedResourceStream = assembly.GetManifestResourceStream("GameThing.Data.categories.json");
			if (embeddedResourceStream == null)
				return;

			using (var streamReader = new StreamReader(embeddedResourceStream))
				LoadCategories(streamReader.ReadToEnd());
		}

		internal void LoadCategories(string jsonString)
		{
			categories = JsonConvert.DeserializeObject<List<Category>>(jsonString);

			var categoryIds = categories.Select(category => category.Id);
			if (categoryIds.Distinct().Count() != categoryIds.Count())
				throw new Exception("One of the categories IDs in categories.json is not unique.");
		}

		public Category Get(string id)
		{
			return categories.Single(category => category.Id.Equals(id));
		}
	}
}
