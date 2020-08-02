using System;
using CitizenFX.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Linq;
using CitizenFX.Core.Native;
using System.Dynamic;

namespace racing
{
	public static partial class UGC
	{
		internal static JArray emptyArray = new JArray();

		internal static bool ContainsKeys(this JObject o, params string[] keys)
		{
			return keys.All(key => o.ContainsKey(key));
		}

		internal static object TryGet(this JObject o, string key, object defaultValue)
		{
			return o.ContainsKey(key) ? o[key] : defaultValue;
		}

		public static Vector3 ToVector3(this JToken t)
		{
			if (t.Type == JTokenType.Object)
			{
				var o = (JObject)t;
				if (o.HasValues && o.ContainsKeys("x", "y", "z"))
					return new Vector3((float)o["x"], (float)o["y"], (float)o["z"]);
			}

			return Vector3.Zero;
		}

		[Serializable]
		public struct CheckpointDefinition
		{
			public Vector3 Location;
			public float Heading;
			public float Scale;
			public bool IsRound;

			/*
			public CheckpointDefinition(Vector3 l, float h, float s, bool iR)
			{
				Location = l;
				Heading = h;
				Scale = s;
				IsRound = iR;
			}

			
			public CheckpointDefinition(dynamic obj)
			{
				try
				{
					Location = obj.Location;
					Heading = obj.Heading;
					Scale = obj.Scale;
					IsRound = obj.IsRound;
				} catch(Exception e)
				{
					throw new ArgumentException("Given object did not contain the data necessary to construct a CheckpointDefinition", "obj", e);
				}
			}*/
		}

		public struct UGCData
		{
			public JObject raw { get; private set; }
			public JObject Mission { get; }
			public JObject Race { get; }
			public JObject Prop { get; }
			//public List<CheckpointDefinition> Checkpoints { get; private set; }
			//public List<PropDefinition> Props { get; private set; }

			public UGCData(JObject rawData)
			{
				raw = rawData;
				Mission = (JObject)raw["mission"];
				Race = (JObject)Mission["race"];
				Prop = (JObject)Mission["prop"];
				//Props = new List<PropDefinition>();
				//Checkpoints = new List<CheckpointDefinition>();


			}
		}

		public static UGCData ParseUGC(string ugcJsonString)
		{
			JObject rawData = JObject.Parse(ugcJsonString);
			var race = new UGCData(rawData);

			return race;
		}
	}
}
