using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GameThing.Entities;

namespace GameThing.Contract
{
	[DataContract]
	public class BattleData
	{
		public const string TestPlayerOneId = "p1";
		public const string TestPlayerTwoId = "p2";

		[DataMember]
		public int TurnNumber { get; set; } = 1;

		[DataMember]
		public int RoundNumber { get; set; } = 0;

		[DataMember]
		public List<Character> Characters { get; set; } = new List<Character>();

		[DataMember]
		public Dictionary<string, CharacterSide> Sides { get; set; } = new Dictionary<string, CharacterSide>();

		[DataMember]
		public string CurrentPlayerId { get; set; }

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

		[DataMember]
		public string LastPlayingPlayerId { get; set; }

		public string GetPlayerId()
		{
			return IsTestMode ? CurrentPlayerId : ApplicationData.PlayerId;
		}

		public CharacterSide? WinnerSide => Sides.ContainsKey(WinnerParticipantId) ? Sides[WinnerParticipantId] : (CharacterSide?) null;

		public void SetWinnerSide(CharacterSide side)
		{
			Status = BattleStatus.Finished;
			WinnerParticipantId = Sides.SingleOrDefault(keyValuePair => keyValuePair.Value == side).Key;
		}

		public void ChangePlayingSide()
		{
			CurrentPlayerId = GetOtherPlayerId();
		}

		private string GetOtherPlayerId()
		{
			return Sides.Keys.Single(key => key != CurrentPlayerId);
		}

		public bool OtherSideHasNoRemainingCharactersAndIHaveSome
		{
			get
			{
				var iHaveCharactersToActivate = Characters.Any(character => character.OwnerPlayerId == CurrentPlayerId && !character.ActivatedThisRound);
				if (!iHaveCharactersToActivate)
					return false;

				var otherPlayerId = GetOtherPlayerId();
				return !Characters.Any(character => character.OwnerPlayerId == otherPlayerId && !character.ActivatedThisRound);
			}
		}

		public bool HasBothSidesAdded => Sides.Count == 2;
		public bool AnyCharacterUnactivated => Characters.Any(character => !character.ActivatedThisRound);

		public void InitializeCharacters(string playerId, TeamData teamData, bool isTestMode = false)
		{
			teamData.Characters.ForEach(character =>
			{
				var cloneCharacter = Convert.Clone(character);
				cloneCharacter.InitializeDeckForBattle();
				cloneCharacter.ResetTurn();

				if (isTestMode)
					cloneCharacter.OwnerPlayerId = playerId;    // Change player ID in test mode

				Characters.Add(cloneCharacter);
			});

			var currentSide = Sides.Keys.Count == 0 ? CharacterSide.Spaghetti : CharacterSide.Unicorn;
			Sides.Add(playerId, currentSide);
		}
	}
}
