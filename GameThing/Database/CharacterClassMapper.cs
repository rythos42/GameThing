using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameThing.Entities;
using Newtonsoft.Json;

namespace GameThing.Database
{
	public interface ICharacterClassMapper : IIdentifiableMapper<CharacterClass>
	{
		void Load();
	}

	public class CharacterClassMapper : ICharacterClassMapper
	{
		private static CharacterClassMapper instance;
		private List<CharacterClass> classes;

		public static CharacterClassMapper Instance
		{
			get
			{
				if (instance == null)
					instance = new CharacterClassMapper();
				return instance;
			}
		}

		private CharacterClassMapper() { }

		public void Load()
		{
			var assembly = Assembly.GetAssembly(typeof(CharacterClassMapper));
			var embeddedResourceStream = assembly.GetManifestResourceStream("GameThing.Data.classes.json");
			if (embeddedResourceStream == null)
				return;

			using (var streamReader = new StreamReader(embeddedResourceStream))
				LoadClasses(streamReader.ReadToEnd());
		}

		internal void LoadClasses(string jsonString)
		{
			classes = JsonConvert.DeserializeObject<List<CharacterClass>>(jsonString);

			var classIds = classes.Select(category => category.Id);
			if (classIds.Distinct().Count() != classIds.Count())
				throw new Exception("One of the class IDs in classes.json is not unique.");

			try
			{
				classes.SelectMany(characterClass => characterClass.StartingCards).ToList().ForEach(cardId => CardMapper.Instance.Get(cardId));
			}
			catch (Exception)
			{
				throw new Exception("One of the starting card IDs in classes.json does not exist in cards.json.");
			}
		}

		public CharacterClass Get(string id)
		{
			return classes.Single(characterClass => characterClass.Id.Equals(id));
		}
	}
}
