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
		/// <summary>
		/// This empty <see cref="JArray"/> is used as a placeholder for missing data when reading from UGC
		/// </summary>
		internal static JArray missingData = new JArray();

		internal static bool ContainsKeys(this JObject o, params string[] keys)
		{
			return keys.All(key => o.ContainsKey(key));
		}

		internal static object TryGet(this JObject o, string key, object defaultValue)
		{
			return o.ContainsKey(key) ? o[key] : defaultValue;
		}

		internal static JArray TryGetArray(this JObject o, string key, JArray defaultValue = null)
		{
			if (o.ContainsKey(key))
			{
				var potentialArray = o[key];
				if (potentialArray.Type == JTokenType.Array)
					return (JArray)potentialArray;
			}

			return defaultValue ?? null;
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
			//public int WrongWayTimer;
			//public CheckpointDefinition SecondCheckpoint;


			/* // All values relating to checkpoints
			Var10 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chh");		// Heading
			iVar11 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chs");		// Scale
			iVar12 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chs2");		// Scale (2)
			iVar13 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chvs");		//
			iVar14 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chpp");		//
			iVar15 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chpps");		//
			iVar16 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chl");		// Location
			iVar17 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "sndchk");	// Location (2)
			iVar18 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "sndrsp");	// 
			iVar19 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpwwt");		// Wrong Way Time
			iVar20 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cppsst");	
			iVar21 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpado");
			iVar22 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpados");
			iVar23 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chttu");
			iVar24 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chttr");
			iVar25 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpbs1");		
			iVar26 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpbs2");
			iVar27 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cptfrm");
			iVar28 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cptfrms");
			iVar29 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "trfmvm");
			iVar30 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chdlo");
			iVar31 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chsto");
			iVar32 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chdlos");
			iVar33 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chstos");
			iVar34 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "rsp");
			iVar35 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cdsblcu");
			iVar36 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpdss");
			iVar37 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "rndchk");	// Is Round
			iVar38 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "rndchks");	// Is Round (2)
			iVar39 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpwtr");
			iVar40 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpwtrs");
			iVar41 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpair");		// Has second checkpoint
			*/

			/* // Old Constructors
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
			public JObject Mission { get => (JObject)raw["mission"]; }
			public JObject Race { get => (JObject)raw["mission"]["race"]; }
			public JObject Prop { get => (JObject)raw["mission"]["prop"]; }
			//public List<CheckpointDefinition> Checkpoints { get; private set; }
			//public List<PropDefinition> Props { get; private set; }

			public UGCData(JObject rawData)
			{
				raw = rawData;
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
