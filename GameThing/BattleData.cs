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

		[DataMember]
		public List<GameLogEntry> GameLog { get; set; } = new List<GameLogEntry>();

		public void ChangePlayingSide()
		{
			CurrentSidesTurn = CurrentSidesTurn == CharacterSide.Spaghetti ? CharacterSide.Unicorn : CharacterSide.Spaghetti;
		}

		public bool OtherSideHasNoRemainingCharactersAndIHaveSome
		{
			get
			{
				var iHaveCharactersToActivate = Characters.Any(character => character.Side == CurrentSidesTurn && !character.ActivatedThisRound);
				if (!iHaveCharactersToActivate)
					return false;

				var otherSide = CurrentSidesTurn == CharacterSide.Spaghetti ? CharacterSide.Unicorn : CharacterSide.Spaghetti;
				return !Characters.Any(character => character.Side == otherSide && !character.ActivatedThisRound);
			}
		}

		public string GetParticipantIdForCurrentSide()
		{
			return Sides.SingleOrDefault(keyValuePair => keyValuePair.Value == CurrentSidesTurn).Key;
		}
	}
}
