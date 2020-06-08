using System.Runtime.Serialization;
using GameThing.Contract;
using GameThing.Entities;

namespace GameThing
{
	[DataContract]
	public class GameLogEntry
	{
		[DataMember]
		public CharacterSide SourceCharacterSide { get; set; }

		[DataMember]
		public CharacterColour SourceCharacterColour { get; set; }

		public Character SourceCharacter
		{
			set
			{
				SourceCharacterSide = value.Side;
				SourceCharacterColour = value.Colour;
			}
		}

		[DataMember]
		public CharacterSide TargetCharacterSide { get; set; }

		[DataMember]
		public CharacterColour TargetCharacterColour { get; set; }

		public Character TargetCharacter
		{
			set
			{
				TargetCharacterSide = value.Side;
				TargetCharacterColour = value.Colour;
			}
		}

		[DataMember]
		public MapPoint MovedTo { get; set; }

		[DataMember]
		public string CardId { get; set; }
	}
}
