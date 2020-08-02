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

		public static List<CheckpointDefinition> GetCheckpointDefinitions(JObject rd)
		{
			// Fail real quick if we've been given invalid data
			if (!rd.ContainsKeys("chp", "chh", "chl"))
				throw new ArgumentException("Given race data is missing required keys for checkpoints (chp, chh, chl)", "rd");

			var checkpointDefinitions = new List<CheckpointDefinition>();

			var numCheckpoints = (int)rd["chp"];

			var heading =	rd.TryGetArray("chh");
			var location =	rd.TryGetArray("chl");
			var scale =		rd.TryGetArray("chs"); //var hasScale = (scale != missingData);
			var isRound =	rd.TryGetArray("rndchk"); //var hasRoundFlag = (isRound != missingData);

			for (int i = 0; i < numCheckpoints; i++)
			{
				var newCheckpoint = new CheckpointDefinition();
				newCheckpoint.IsRound =		(bool)		isRound?[i];
				newCheckpoint.Location =	(Vector3)	location?[i].ToVector3() + checkpointPositionOffset[newCheckpoint.IsRound ? "round" : "normal"];
				newCheckpoint.Heading =		(float)		heading?[i];
				newCheckpoint.Scale =		(float)		scale?[i];

				checkpointDefinitions.Add(newCheckpoint);
			}

			return checkpointDefinitions;
		}
	}
}
