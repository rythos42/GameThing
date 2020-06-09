using System.Runtime.Serialization;
using GameThing.Contract;

namespace GameThing
{
	[DataContract]
	public class GameLogEntry
	{
		[DataMember]
		public CharacterSide SourceCharacterSide { get; set; }

		[DataMember]
		public CharacterColour SourceCharacterColour { get; set; }

		[DataMember]
		public CharacterSide TargetCharacterSide { get; set; }

		[DataMember]
		public CharacterColour TargetCharacterColour { get; set; }

		[DataMember]
		public MapPoint MovedTo { get; set; }

		[DataMember]
		public string CardId { get; set; }
	}
}
