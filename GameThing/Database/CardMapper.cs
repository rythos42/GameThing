using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameThing.Entities.Cards;
using Newtonsoft.Json;

namespace GameThing.Database
{
	public interface ICardMapper : IIdentifiableMapper<Card>
	{
		void Load();
	}

	public class CardMapper : ICardMapper
	{
		private static ICardMapper instance;
		private List<Card> cards;

		public static ICardMapper Instance
		{
			get
			{
				if (instance == null)
					instance = new CardMapper();
				return instance;
			}
			internal set
			{
				instance = value;
			}
		}

		private CardMapper() { }

		public void Load()
		{
			var assembly = Assembly.GetAssembly(typeof(CardMapper));
			var embeddedResourceStream = assembly.GetManifestResourceStream("GameThing.Data.cards.json");
			if (embeddedResourceStream == null)
				return;

			using (var streamReader = new StreamReader(embeddedResourceStream))
				LoadCards(streamReader.ReadToEnd());
		}

		internal void LoadCards(string jsonString)
		{
			cards = JsonConvert.DeserializeObject<List<Card>>(jsonString);

			var cardIds = cards.Select(card => card.Id);
			if (cardIds.Distinct().Count() != cardIds.Count())
				throw new Exception("One of the card IDs in cards.json is not unique.");
		}

		public Card Get(string id)
		{
			return cards.Single(card => card.Id == id);
		}
	}
}
