using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GameThing.Entities;
using Newtonsoft.Json;

namespace GameThing.Data
{
	[DataContract]
	public class BattleData
	{
		[DataMember]
		public int TurnNumber { get; set; } = 0;

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
		public string WinnerParticipantId { get; set; } = null;

		[DataMember]
		public List<GameLogEntry> GameLog { get; set; } = new List<GameLogEntry>();

		[DataMember]
		public bool IsTestMode { get; set; }

		[DataMember]
		public BattleStatus Status { get; set; }

		public CharacterSide? WinnerSide
		{
			get
			{
				return Sides.ContainsKey(WinnerParticipantId) ? Sides[WinnerParticipantId] : (CharacterSide?) null;
			}
		}

		public void SetWinnerSide(CharacterSide side)
		{
			Status = BattleStatus.Finished;
			WinnerParticipantId = Sides.SingleOrDefault(keyValuePair => keyValuePair.Value == side).Key;
		}

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

		public bool HasBothSidesAdded
		{
			get { return Sides.Count == 2; }
		}

		public bool HasMySideAdded(string participantId)
		{
			return Sides.ContainsKey(participantId);
		}

		public void InitializeCharacters(string participantId, TeamData teamData)
		{
			teamData.Characters.ForEach(character =>
			{
				var cloneCharacter = Clone(character);
				cloneCharacter.Side = CurrentSidesTurn;
				cloneCharacter.InitializeDeckForBattle();
				cloneCharacter.ResetTurn();
				Characters.Add(cloneCharacter);
			});

			Sides.Add(participantId, CurrentSidesTurn);
		}

		private Character Clone(Character character)
		{
			var jsonSettings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore,
				Formatting = Formatting.None,
				TypeNameHandling = TypeNameHandling.Objects
			};

			var serialized = JsonConvert.SerializeObject(character, jsonSettings);
			return JsonConvert.DeserializeObject<Character>(serialized, jsonSettings);
		}
	}
}
