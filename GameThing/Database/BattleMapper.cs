using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Streaming;
using GameThing.Contract;

namespace GameThing.Database
{
	public class BattleMapper
	{
		private readonly FirebaseDatabase<BattleData> firebase;

		public BattleMapper(string firebaseUrl)
		{
			firebase = new FirebaseDatabase<BattleData>(firebaseUrl, "Battle");
		}

		public async Task<BattleData> GetBattle(string matchId)
		{
			return await firebase.Get(matchId);
		}

		public async Task PutBattle(BattleData data)
		{
			await firebase.Save(data.MatchId, data);
		}

		public IObservable<FirebaseEvent<BattleData>> ObserveBattle(string matchId)
		{
			return firebase.Observe(matchId);
		}

		public async Task<IReadOnlyCollection<FirebaseObject<BattleData>>> GetBattles()
		{
			return await firebase.GetList();
		}
	}
}
