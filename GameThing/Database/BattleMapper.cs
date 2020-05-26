using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using GameThing.Data;

namespace GameThing.Database
{
	public class BattleMapper
	{
		private readonly FirebaseClient firebase;

		public BattleMapper(string firebaseUrl)
		{
			firebase = new FirebaseClient(firebaseUrl);
		}

		public async Task<BattleData> GetBattle(string matchId)
		{
			return await firebase.Child($"Battle/{matchId}").OnceSingleAsync<BattleData>();
		}

		public async Task PutBattle(BattleData data)
		{
			await firebase.Child($"Battle/{data.MatchId}").PutAsync(data);
		}

		public IObservable<FirebaseEvent<BattleData>> ObserveBattle(string matchId)
		{
			return firebase
				.Child($"Battle/{matchId}")
				.AsObservable<BattleData>(delegate (object sender, ContinueExceptionEventArgs<FirebaseException> ex)
				{
					Console.WriteLine(ex.ToString());
				}, matchId);
		}

		public async Task<IReadOnlyCollection<FirebaseObject<BattleData>>> GetBattles()
		{
			return await firebase.Child("Battle").OnceAsync<BattleData>();
		}
	}
}
