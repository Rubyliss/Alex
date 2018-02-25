﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ResourcePackLib.Json.Converters
{

	public class JVector3Converter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var v = value as JVector3;

			serializer.Serialize(writer, new float[]
			{
				v.X,
				v.Y,
				v.Z
			});
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type == JTokenType.Array)
			{
				var arr = (JArray)obj;
				if (arr.Count == 3 && arr.All(token => token.Type == JTokenType.Integer))
				{
					return new JVector3()
					{
						X = arr[0].Value<int>(),
						Y = arr[1].Value<int>(),
						Z = arr[2].Value<int>()
					};
				}
				else if(arr.Count == 3 && arr.All(token => token.Type == JTokenType.Float))
				{
					return new JVector3()
					{
						X = arr[0].Value<float>(),
						Y = arr[1].Value<float>(),
						Z = arr[2].Value<float>()
					};
				}
			}

			return null;
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(JVector3).IsAssignableFrom(objectType);
		}
	}
}