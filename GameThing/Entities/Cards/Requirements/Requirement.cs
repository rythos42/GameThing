using System;

namespace GameThing.Entities.Cards.Requirements
{
	public class Requirement
	{
		public RequirementType Type { get; set; }

		public bool Met(Character source, Character target)
		{
			switch (Type)
			{
				case RequirementType.BehindTarget:
					return BehindTargetRequirementMet(source, target);
				default:
					throw new Exception($"Unknown requirement type {Type}.");
			}
		}

		private bool BehindTargetRequirementMet(Character source, Character target)
		{
			var northSouthDiff = source.MapPosition.Y - target.MapPosition.Y;
			var eastWestDiff = source.MapPosition.X - target.MapPosition.X;

			switch (target.Facing)
			{
				case CharacterFacing.North: return northSouthDiff > 0;
				case CharacterFacing.South: return northSouthDiff < 0;
				case CharacterFacing.West: return eastWestDiff > 0;
				case CharacterFacing.East: return eastWestDiff < 0;
			}

			return false;
		}
	}
}
