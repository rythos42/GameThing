using System.Collections.Generic;
using GameThing.Contract;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Animations.SpriteSheets;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.Tiled;

namespace GameThing.Entities
{
	public class Content
	{
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

			//Map = contentManager.Load<TiledMap>("tilemaps/Map");
			Map = contentManager.Load<TiledMap>("tilemaps/ComplexMap");

			spaghettiFactory = CreateAnimationFactory(contentManager, CharacterSide.Spaghetti);
			unicornFactory = CreateAnimationFactory(contentManager, CharacterSide.Unicorn);
		}

		private SpriteSheetAnimationFactory CreateAnimationFactory(ContentManager contentManager, CharacterSide side)
		{
			var sideString = side.ToString().ToLower();
			var texture = contentManager.Load<Texture2D>($"sprites/{sideString}/sprites");
			var map = contentManager.Load<Dictionary<string, Rectangle>>($"sprites/{sideString}/spritesMap");
			var atlas = new TextureAtlas("character", texture, map);
			var factory = new SpriteSheetAnimationFactory(atlas);

			int start = -1;
			factory.Add(GetSpriteTag(CharacterColour.Blue, CharacterFacing.East), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.Blue, CharacterFacing.North), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.Blue, CharacterFacing.South), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.Blue, CharacterFacing.West), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.Green, CharacterFacing.East), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.Green, CharacterFacing.North), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.Green, CharacterFacing.South), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.Green, CharacterFacing.West), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.None, CharacterFacing.East), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.None, CharacterFacing.North), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.None, CharacterFacing.South), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.None, CharacterFacing.West), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.Red, CharacterFacing.East), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.Red, CharacterFacing.North), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.Red, CharacterFacing.South), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.Red, CharacterFacing.West), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.White, CharacterFacing.East), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.White, CharacterFacing.North), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.White, CharacterFacing.South), new SpriteSheetAnimationData(Next6(ref start)));
			factory.Add(GetSpriteTag(CharacterColour.White, CharacterFacing.West), new SpriteSheetAnimationData(Next6(ref start)));

			return factory;
		}

		private int[] Next6(ref int start)
		{
			return new[] { ++start, ++start, ++start, ++start, ++start, ++start };
		}

		private readonly SpriteSheetAnimationFactory spaghettiFactory;
		private readonly SpriteSheetAnimationFactory unicornFactory;

		public SpriteSheetAnimationFactory GetAnimationFactory(CharacterSide side)
		{
			return side == CharacterSide.Spaghetti ? spaghettiFactory : unicornFactory;
		}

		public string GetSpriteTag(CharacterColour colour, CharacterFacing facing)
		{
			return $"{colour.ToString().ToLower()}_{facing.ToString().ToLower()}";
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
	}
}
