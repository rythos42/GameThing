using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace GameThing.Database
{
	public interface INameMapper
	{
		void Load();
		string GetRandom();
	}

	public class NameMapper : INameMapper
	{
		private static INameMapper instance;
		private List<string> names;
		private IRandomWrapper random;

		public static INameMapper Instance
		{
			get
			{
				if (instance == null)
					instance = new NameMapper();
				return instance;
			}
			internal set
			{
				instance = value;
			}
		}

		private NameMapper() { }

		public void Load()
		{
			var assembly = Assembly.GetAssembly(typeof(CardMapper));
			var embeddedResourceStream = assembly.GetManifestResourceStream("GameThing.Data.names.json");
			if (embeddedResourceStream == null)
				return;

			using (var streamReader = new StreamReader(embeddedResourceStream))
				LoadCards(streamReader.ReadToEnd());
		}

		internal void LoadCards(string jsonString)
		{
			names = JsonConvert.DeserializeObject<List<string>>(jsonString);
			random = new RandomWrapper();
		}

		public string GetRandom()
		{
			var index = random.Next(names.Count - 1);
			return names[index];
		}
	}
}
