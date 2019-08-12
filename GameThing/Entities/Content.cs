using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities
{
	public class Content
	{
		private readonly Dictionary<string, Texture2D> characterSprites = new Dictionary<string, Texture2D>();

		public Content(ContentManager content)
		{
			Font = content.Load<SpriteFont>("fonts/Carlito-Regular");

			Card = content.Load<Texture2D>("sprites/card");
			DistanceOverlay = content.Load<Texture2D>("sprites/distance_overlay");
			Lock = content.Load<Texture2D>("sprites/lock");

			Highlight = content.Load<Effect>("effects/highlight");

			foreach (CharacterSide side in Enum.GetValues(typeof(CharacterSide)))
			{
				foreach (CharacterColour colour in Enum.GetValues(typeof(CharacterColour)))
				{
					characterSprites[CreateKey(side, colour)] = content.Load<Texture2D>($"sprites/{side.ToString().ToLower()}_atlas_{colour.ToString().ToLower()}");
				}
			}
		}

		public SpriteFont Font { get; private set; }

		public Texture2D DistanceOverlay { get; private set; }
		public Texture2D Card { get; private set; }
		public Texture2D Lock { get; private set; }

		public Effect Highlight { get; private set; }

		public Texture2D GetSpriteFor(Character character)
		{
			return characterSprites[CreateKey(character.Side, character.Colour)];
		}

		private string CreateKey(CharacterSide side, CharacterColour colour)
		{
			return $"{side}:{colour}";
		}
	}
}
