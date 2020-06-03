using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using GameThing.Contract;

namespace GameThing.Database
{
	public class TeamMapper
	{
		private readonly FirebaseClient firebase;

		public TeamMapper(string firebaseUrl)
		{
			firebase = new FirebaseClient(firebaseUrl);
		}

		public async Task<TeamData> GetTeam(string playerId)
		{
			return await firebase.Child($"Team/{playerId}").OnceSingleAsync<TeamData>();
		}

		public async Task SaveTeam(string playerId, TeamData data)
		{
			await firebase.Child($"Team/{playerId}").PutAsync(data);
		}

		public async Task DeleteTeam(string playerId)
		{
			await firebase.Child($"Team/{playerId}").DeleteAsync();
		}
	}
}
