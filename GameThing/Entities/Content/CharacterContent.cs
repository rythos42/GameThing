using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.Entities.Content
{
	public class CharacterContent
	{
		private readonly Dictionary<string, Texture2D> sprites = new Dictionary<string, Texture2D>();
		private readonly CommonContent common;

		public CharacterContent(ContentManager content, CommonContent common)
		{
			this.common = common;

			foreach (CharacterSide side in Enum.GetValues(typeof(CharacterSide)))
			{
				foreach (CharacterColour colour in Enum.GetValues(typeof(CharacterColour)))
				{
					sprites[CreateKey(side, colour)] = content.Load<Texture2D>($"sprites/{side.ToString().ToLower()}_atlas_{colour.ToString().ToLower()}");
				}
			}
		}

		public Texture2D GetSpriteFor(Character character)
		{
			return sprites[CreateKey(character.Side, character.Colour)];
		}

		private string CreateKey(CharacterSide side, CharacterColour colour)
		{
			return $"{side}:{colour}";
		}

		public Texture2D AvailableMovementTexture
		{
			get { return common.DistanceOverlay; }
		}
	}
}
