using System.Collections.Generic;
using System.Linq;
using GameThing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameThing.UI
{
	public class AppliedConditionRow : UIComponent
	{
		private List<Texture2D> conditionIcons;

		public Character SelectedCharacter { get; set; }

		public override void Draw(SpriteBatch spriteBatch, float x, float y)
		{
			X = x;
			Y = y;
			Dimensions = MeasureContent();

			if (SelectedCharacter == null)
				return;

			SelectedCharacter.Conditions.ForEach(appliedCondition =>
			{
				var icon = GetIcon(appliedCondition.Condition.IconName);
				spriteBatch.Draw(icon, new Vector2(x, y), Color.White);
				x += icon.Width;
			});

			IsVisible = true;
		}

		private Texture2D GetIcon(string name)
		{
			return conditionIcons.SingleOrDefault(conditionIcon => conditionIcon.Name == name);
		}

		public override void LoadContent(Content content, GraphicsDevice graphicsDevice)
		{
			conditionIcons = content.ConditionIcons;
		}

		public override Vector2 MeasureContent()
		{
			if (SelectedCharacter == null || SelectedCharacter.Conditions.Count == 0)
				return Vector2.Zero;

			return new Vector2(SelectedCharacter.Conditions.Count * conditionIcons[0].Width, conditionIcons[0].Height);
		}
	}
}
