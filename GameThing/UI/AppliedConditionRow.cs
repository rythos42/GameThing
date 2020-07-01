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

		public override void Draw(SpriteBatch spriteBatch)
		{
			Dimensions = SelectedCharacter == null || SelectedCharacter.Conditions.Count == 0
				? Vector2.Zero
				: new Vector2(SelectedCharacter.Conditions.Count * conditionIcons[0].Width, conditionIcons[0].Height);

			if (SelectedCharacter == null)
				return;

			var drawX = X;
			SelectedCharacter.Conditions.ForEach(appliedCondition =>
			{
				var icon = GetIcon(appliedCondition.Condition.IconName);
				spriteBatch.Draw(icon, new Vector2(drawX, Y), Color.White);
				drawX += icon.Width;
			});

			IsVisible = true;
		}

		private Texture2D GetIcon(string name)
		{
			return conditionIcons.SingleOrDefault(conditionIcon => conditionIcon.Name == name);
		}

		protected override void LoadComponentContent(Content content, GraphicsDevice graphicsDevice)
		{
			conditionIcons = content.ConditionIcons;
		}
	}
}
