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
			CardSprite = content.Load<Texture2D>("sprites/card");
			Font = content.Load<SpriteFont>("fonts/Carlito-Regular");
			DistanceOverlay = content.Load<Texture2D>("sprites/distance_overlay");
			GaussianBlur = content.Load<Effect>("effects/gaussian_blur");

			foreach (CharacterSide side in Enum.GetValues(typeof(CharacterSide)))
			{
				foreach (CharacterColour colour in Enum.GetValues(typeof(CharacterColour)))
				{
					characterSprites[CreateKey(side, colour)] = content.Load<Texture2D>($"sprites/{side.ToString().ToLower()}_atlas_{colour.ToString().ToLower()}");
				}
			}
		}

		public Texture2D DistanceOverlay { get; private set; }
		public Effect GaussianBlur { get; private set; }

		public Texture2D CardSprite { get; private set; }
		public SpriteFont Font { get; private set; }

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
