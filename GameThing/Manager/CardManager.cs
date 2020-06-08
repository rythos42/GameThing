using System.Collections.Generic;
using System.Linq;
using GameThing.Database;
using GameThing.Entities;
using GameThing.Entities.Cards;

namespace GameThing.Manager
{
	public class CardManager
	{
		private static CardManager instance;

		public static CardManager Instance
		{
			get
			{
				if (instance == null)
					instance = new CardManager();
				return instance;
			}
		}

		private CardManager() { }

		public Card GetCard(string id)
		{
			return CardMapper.Instance.Get(id);
		}

		public List<Card> CreateDefaultDeck(Character character)
		{
			return character.CharacterClass.StartingCards.Select(cardId => GetCard(cardId).CreateForCharacter(character)).ToList();
		}
	}
}
