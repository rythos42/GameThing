using System;
using System.Collections.Generic;
using GameThing.Contract;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;

namespace GameThing.Entities
{
	public class Content
	{
		private readonly Dictionary<string, Texture2D> characterSprites = new Dictionary<string, Texture2D>();

		public Content(ContentManager contentManager)
		{
			Font = contentManager.Load<SpriteFont>("fonts/Carlito-Regular");
			PatrickHandSc = contentManager.Load<SpriteFont>("fonts/PatrickHandSC-Regular");

			Card = contentManager.Load<Texture2D>("sprites/card");
			DistanceOverlay = contentManager.Load<Texture2D>("sprites/distance_overlay");

			LargeBush = contentManager.Load<Texture2D>("sprites/terrain/large_bush");
			SmallTree = contentManager.Load<Texture2D>("sprites/terrain/small_tree");
			MediumTree = contentManager.Load<Texture2D>("sprites/terrain/medium_tree");

			MainBackground = contentManager.Load<Texture2D>("sprites/backgrounds/main");

			Lock = contentManager.Load<Texture2D>("sprites/icons/lock");
			SpaghettiHatIcon = contentManager.Load<Texture2D>("sprites/icons/spaghetti_atlas_hat");
			UnicornHatIcon = contentManager.Load<Texture2D>("sprites/icons/unicorn_atlas_hat");
			MoveIcon = contentManager.Load<Texture2D>("sprites/icons/move");
			CardIcon = contentManager.Load<Texture2D>("sprites/icons/card");

			ConditionIcons.Add(contentManager.Load<Texture2D>("sprites/icons/buff"));
			ConditionIcons.Add(contentManager.Load<Texture2D>("sprites/icons/distract"));
			ConditionIcons.Add(contentManager.Load<Texture2D>("sprites/icons/taunt"));
			ConditionIcons.Add(contentManager.Load<Texture2D>("sprites/icons/run"));

			ButtonLeft = contentManager.Load<Texture2D>("sprites/ui/button_left");
			ButtonRight = contentManager.Load<Texture2D>("sprites/ui/button_right");
			ButtonTopBottom = contentManager.Load<Texture2D>("sprites/ui/button_top_bottom");
			ButtonLeftDisabled = contentManager.Load<Texture2D>("sprites/ui/button_left_disabled");
			ButtonRightDisabled = contentManager.Load<Texture2D>("sprites/ui/button_right_disabled");
			ButtonTopBottomDisabled = contentManager.Load<Texture2D>("sprites/ui/button_top_bottom_disabled");
			PanelBackground = contentManager.Load<Texture2D>("sprites/ui/panel");

			Highlight = contentManager.Load<Effect>("effects/highlight");
			Shade = contentManager.Load<Effect>("effects/shade");

			foreach (CharacterSide side in Enum.GetValues(typeof(CharacterSide)))
			{
				foreach (CharacterColour colour in Enum.GetValues(typeof(CharacterColour)))
				{
					var sideLower = side.ToString().ToLower();
					characterSprites[CreateKey(side, colour)] = contentManager.Load<Texture2D>($"sprites/{sideLower}/{sideLower}_atlas_{colour.ToString().ToLower()}_south_0");
				}
			}

			//Map = contentManager.Load<TiledMap>("tilemaps/Map");
			Map = contentManager.Load<TiledMap>("tilemaps/ComplexMap");
		}

		public SpriteFont Font { get; private set; }
		public SpriteFont PatrickHandSc { get; private set; }

		public Texture2D DistanceOverlay { get; private set; }
		public Texture2D Card { get; private set; }
		public Texture2D Lock { get; private set; }

		public Texture2D MainBackground { get; private set; }

		public Texture2D LargeBush { get; private set; }
		public Texture2D SmallTree { get; private set; }
		public Texture2D MediumTree { get; private set; }

		public Texture2D SpaghettiHatIcon { get; private set; }
		public Texture2D UnicornHatIcon { get; private set; }
		public Texture2D MoveIcon { get; private set; }
		public Texture2D CardIcon { get; private set; }
		public List<Texture2D> ConditionIcons { get; private set; } = new List<Texture2D>();

		public Texture2D ButtonLeft { get; private set; }
		public Texture2D ButtonRight { get; private set; }
		public Texture2D ButtonTopBottom { get; private set; }
		public Texture2D ButtonLeftDisabled { get; private set; }
		public Texture2D ButtonRightDisabled { get; private set; }
		public Texture2D ButtonTopBottomDisabled { get; private set; }
		public Texture2D PanelBackground { get; private set; }

		public Effect Highlight { get; private set; }
		public Effect Shade { get; private set; }

		public TiledMap Map { get; private set; }

		public Texture2D GetSpriteFor(Character character, CharacterSide side)
		{
			return characterSprites[CreateKey(side, character.Colour)];
		}

		private string CreateKey(CharacterSide side, CharacterColour colour)
		{
			return $"{side}:{colour}";
		}
	}
}
