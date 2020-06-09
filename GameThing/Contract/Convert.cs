using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace GameThing.Contract
{
	public static class Convert
	{
		private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.Ignore,
			Formatting = Formatting.None,
			TypeNameHandling = TypeNameHandling.Objects
		};

		public static T Clone<T>(T obj)
		{
			return Deserialize<T>(Serialize(obj));
		}

		public static byte[] Serialize<T>(T objectData)
		{
			var objectJson = JsonConvert.SerializeObject(objectData, jsonSettings);
			var objectBytes = Encoding.UTF8.GetBytes(objectJson);
			using (var outputMemoryStream = new MemoryStream())
			using (var deflateStream = new DeflateStream(outputMemoryStream, CompressionMode.Compress))
			using (var inputMemoryStream = new MemoryStream(objectBytes))
			{
				inputMemoryStream.CopyTo(deflateStream);
				deflateStream.Close();
				return outputMemoryStream.ToArray();
			}
		}

		public static T Deserialize<T>(byte[] objectBytes)
		{
			using (var inputMemoryStream = new MemoryStream(objectBytes))
			using (var deflateStream = new DeflateStream(inputMemoryStream, CompressionMode.Decompress))
			using (var outputMemoryStream = new MemoryStream())
			{
				deflateStream.CopyTo(outputMemoryStream);
				outputMemoryStream.Close();

				var gameData = outputMemoryStream.ToArray();
				var battleJson = Encoding.UTF8.GetString(gameData);
				return JsonConvert.DeserializeObject<T>(battleJson, jsonSettings);
			}
		}
	}
}
