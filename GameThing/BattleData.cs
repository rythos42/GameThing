using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GameThing.Entities;

namespace GameThing
{
	[DataContract]
	public class BattleData
	{
		[DataMember]
		public int RoundNumber { get; set; } = 0;

		[DataMember]
		public List<Character> Characters { get; set; } = new List<Character>();

		[DataMember]
		public Dictionary<string, CharacterSide> Sides { get; set; } = new Dictionary<string, CharacterSide>();

		[DataMember]
		public CharacterSide CurrentSidesTurn { get; set; }

		[DataMember]
		public string MatchId { get; set; }

		[DataMember]
		public CharacterSide? Winner { get; set; } = null;

		public void ChangePlayingSide()
		{
			CurrentSidesTurn = CurrentSidesTurn == CharacterSide.Spaghetti ? CharacterSide.Unicorn : CharacterSide.Spaghetti;
		}

		public string GetParticipantIdForCurrentSide()
		{
			return Sides.SingleOrDefault(keyValuePair => keyValuePair.Value == CurrentSidesTurn).Key;
		}
	}
}
