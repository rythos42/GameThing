using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameThing.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameThing.Entities.Cards
{
	public class IdentifierBasedConverter<T> : JsonConverter where T : IIdentifiable
	{
		private readonly Type mapperType;
		private static IIdentifiableMapper<T> mapper;

		public IdentifierBasedConverter(Type mapperType)
		{
			this.mapperType = mapperType;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType.IsAssignableFrom(typeof(List<string>));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (mapper == null)
			{
				// Get the public static Instance field that returns a class that can be used to map
				var fieldInfo = mapperType
					.GetProperties(BindingFlags.Static | BindingFlags.Public)
					.Single(field => typeof(IIdentifiableMapper<T>).IsAssignableFrom(field.PropertyType));
				mapper = (IIdentifiableMapper<T>) fieldInfo.GetValue(null);
			}

			if (typeof(IList<T>).IsAssignableFrom(objectType))
			{
				var listType = objectType.GenericTypeArguments[0];
				var idList = serializer.Deserialize<List<string>>(reader);
				return idList.Select(id => mapper.Get(id)).ToList();
			}
			else
			{
				var id = serializer.Deserialize<string>(reader);
				return mapper.Get(id);
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (typeof(IList<T>).IsAssignableFrom(value.GetType()))
			{
				var list = (List<T>) value;
				JArray.FromObject(list.Select(identifiable => identifiable.Id)).WriteTo(writer);
			}
			else
			{
				var identifiable = (T) value;
				JToken.FromObject(identifiable.Id).WriteTo(writer);
			}
		}
	}
}
