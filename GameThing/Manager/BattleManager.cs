using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database.Streaming;
using GameThing.Data;
using GameThing.Database;

namespace GameThing.Manager
{
	public class BattleManager : IDisposable
	{
		private static BattleManager instance;

		private readonly BattleMapper battleMapper = new BattleMapper(ApplicationData.FirebaseUrl);

		private IDisposable watchingBattle;
		private BattleData currentBattle;

		public event BattleDataUpdatedEventHandler DataUpdated;

		public static BattleManager Instance
		{
			get
			{
				if (instance == null)
					instance = new BattleManager();
				return instance;
			}
		}

		private BattleManager() { }

		public async Task<BattleData> CreateBattle(TeamData team)
		{
			var matchId = Guid.NewGuid();
			var battleData = new BattleData
			{
				CurrentSidesTurn = CharacterSide.Spaghetti,
				MatchId = matchId.ToString()
			};

			battleData.InitializeCharacters(ApplicationData.PlayerId, team);
			battleData.ChangePlayingSide();
			await battleMapper.PutBattle(battleData);

			return battleData;
		}

		public async Task<BattleData> JoinBattleAndObserve(string matchId, TeamData team)
		{
			currentBattle = await battleMapper.GetBattle(matchId);
			currentBattle.InitializeCharacters(ApplicationData.PlayerId, team);
			await battleMapper.PutBattle(currentBattle);

			watchingBattle = battleMapper.ObserveBattle(matchId).Subscribe(OnDataUpdated);

			return currentBattle;
		}

		public async Task<BattleData> GetBattleAndObserve(string matchId)
		{
			currentBattle = await battleMapper.GetBattle(matchId);
			watchingBattle = battleMapper.ObserveBattle(matchId).Subscribe(OnDataUpdated);
			return currentBattle;
		}

		private void OnDataUpdated(FirebaseEvent<BattleData> evt)
		{
			currentBattle = evt.Object;

			if (currentBattle.LastPlayingPlayerId != ApplicationData.PlayerId)
				DataUpdated?.Invoke(evt.Object);
		}

		public async Task SaveBattle(BattleData battleData)
		{
			await battleMapper.PutBattle(battleData);
		}

		public async Task<IEnumerable<BattleData>> GetAvailableBattles()
		{
			return (await battleMapper
				.GetBattles())
				.Where(firebase => !firebase.Object.HasBothSidesAdded && !firebase.Object.Sides.ContainsKey(ApplicationData.PlayerId))
				.Select(firebase => firebase.Object);
		}

		public async Task<IEnumerable<BattleData>> GetMyBattles()
		{
			return (await battleMapper
				.GetBattles())
				.Where(firebase => firebase.Object.Sides.ContainsKey(ApplicationData.PlayerId) && firebase.Object.Status != BattleStatus.Finished)
				.Select(firebase => firebase.Object);
		}

		public void Dispose()
		{
			watchingBattle.Dispose();
		}
	}
}
