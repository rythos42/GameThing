namespace GameThing.Database
{
	public interface IIdentifiableMapper<T> where T : IIdentifiable
	{
		T Get(string id);
	}
}
