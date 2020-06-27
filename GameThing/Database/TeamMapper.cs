using System.Threading.Tasks;
using GameThing.Contract;

namespace GameThing.Database
{
	public class TeamMapper
	{
		private readonly FirebaseDatabase<TeamData> firebase;

		public TeamMapper(string firebaseUrl)
		{
			firebase = new FirebaseDatabase<TeamData>(firebaseUrl, "Team");
		}

		public async Task<TeamData> GetTeam(string playerId)
		{
			return await firebase.Get(playerId);
		}

		public async Task SaveTeam(string playerId, TeamData data)
		{
			await firebase.Save(playerId, data);
		}

		public async Task DeleteTeam(string playerId)
		{
			await firebase.Delete(playerId);
		}
	}
}
