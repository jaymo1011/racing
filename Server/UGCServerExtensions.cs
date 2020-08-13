using CitizenFX.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace racing
{
	public static partial class UGC
	{
		internal static Dictionary<string, Vector3> checkpointPositionOffset = new Dictionary<string, Vector3>(){
			{ "normal", new Vector3(0f, 0f, 5f) },
			{ "round", new Vector3(0f, 0f, 10.5f) },
		};

		// 
		public static List<CheckpointDefinition> GetCheckpointDefinitions(this Map map)
		{
			// Fail real quick if we've been given invalid data
			if (map.GetObject("mission.race")?.ContainsKeys("chp", "chh", "chl") != true)
				throw new ArgumentException("The specified map is missing required keys for checkpoints (chp, chh, chl)", "map");

			var checkpointDefinitions = new List<CheckpointDefinition>();

			var numCheckpoints = map.Get<int>("mission.race.chp");

			var heading = map.GetList<float>("mission.race.chh");
			var location = map.GetList<Vector3>("mission.race.chl");
			var scale = map.GetList<float>("mission.race.chs");
			var isRound = map.GetList<bool>("mission.race.rndchk");

			for (int i = 0; i < numCheckpoints; i++)
			{
				var newCheckpoint = new CheckpointDefinition();
				newCheckpoint.IsRound = (bool)isRound?[i];
				newCheckpoint.Location = location[i] + checkpointPositionOffset[newCheckpoint.IsRound ? "round" : "normal"];
				newCheckpoint.Heading = heading[i];
				newCheckpoint.Scale = scale[i];

				checkpointDefinitions.Add(newCheckpoint);
			}

			return checkpointDefinitions;
		}
	}
}
