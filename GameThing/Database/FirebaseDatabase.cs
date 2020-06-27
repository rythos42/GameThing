using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;

namespace GameThing.Database
{
	public class FirebaseDatabase<T> where T : class
	{
		private readonly FirebaseClient firebase;
		private readonly string root;

		public FirebaseDatabase(string url, string root)
		{
			var authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyAXGixccFsbku4uVyzeEsileR3LMqtdvxc"));
			firebase = new FirebaseClient(url,
				new FirebaseOptions
				{
					AuthTokenAsyncFactory = () => authProvider
						.SignInWithEmailAndPasswordAsync("rythos42@gmail.com", "GameThingHEY12")
						.ContinueWith<string>(task => task.Result.FirebaseToken)
				});

			this.root = root;
		}

		public async Task<T> Get(string id)
		{
			return await firebase.Child($"{root}/{id}").OnceSingleAsync<T>();
		}

		public async Task<IReadOnlyCollection<FirebaseObject<T>>> GetList()
		{
			return await firebase.Child(root).OnceAsync<T>();
		}

		public IObservable<FirebaseEvent<T>> Observe(string id)
		{
			return firebase
				.Child($"{root}/{id}")
				.AsObservable<T>(delegate (object sender, ContinueExceptionEventArgs<FirebaseException> ex)
				{
					Console.WriteLine(ex.ToString());
				}, id);
		}

		public async Task Save(string id, T data)
		{
			await firebase.Child($"{root}/{id}").PutAsync(data);
		}

		public async Task Delete(string id)
		{
			await firebase.Child($"{root}/{id}").DeleteAsync();
		}
	}
}
