using System.Collections.Generic;
using System.Runtime.Serialization;
using GameThing.Entities;

namespace GameThing.Data
{
	[DataContract]
	public class TeamData
	{
		[DataMember]
		public List<Character> Characters { get; set; } = new List<Character>();
	}
}
