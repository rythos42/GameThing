using System.Threading.Tasks;
using GameThing.Contract;
using GameThing.Database;

namespace GameThing.Manager
{
	public class TeamManager
	{
		private static TeamManager instance;

		private readonly TeamMapper teamMapper = new TeamMapper(ApplicationData.FirebaseUrl);

		public event TeamLoadedEventHandler OnTeamLoad;

		public static TeamManager Instance
		{
			get
			{
				if (instance == null)
					instance = new TeamManager();
				return instance;
			}
		}

		private TeamManager()
		{
			teamMapper.GetTeam(ApplicationData.PlayerId).ContinueWith(team =>
			{
				HasTeam = !team.IsFaulted && team.Result != null;
				Team = team.Result;
				OnTeamLoad?.Invoke(Team);
			});
		}

		public async Task CreateTeam(TeamData team)
		{
			await teamMapper.SaveTeam(ApplicationData.PlayerId, team);
			HasTeam = true;
			Team = team;
			OnTeamLoad(team);
		}

		public async Task DeleteTeam()
		{
			await teamMapper.DeleteTeam(ApplicationData.PlayerId);
			HasTeam = false;
			Team = null;
		}

		public async Task MergeBattleDataAndSaveTeam(BattleData battleData)
		{
			Team.MergeBattleData(battleData);
			await teamMapper.SaveTeam(ApplicationData.PlayerId, Team);
		}

		public TeamData Team { get; private set; }
		public bool HasTeam { get; private set; }
	}
}
