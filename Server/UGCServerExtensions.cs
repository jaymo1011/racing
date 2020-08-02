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
				throw new ArgumentException("Given race data is missing required keys!", "rd");

			var checkpointDefinitions = new List<CheckpointDefinition>();

			var numCheckpoints = (int)rd["chp"];
			var heading = (JArray)rd["chh"];
			var location = (JArray)rd["chl"];

			var scale = (JArray)rd.TryGet("chs", emptyArray);
			var hasScale = (scale != emptyArray);

			var isRound = (JArray)rd.TryGet("rndchk", emptyArray);
			var hasRoundFlag = (isRound != emptyArray);

			for (int i = 0; i < numCheckpoints; i++)
			{
				/*
				var thisLocation = location[i].ToVector3();
				var thisHeading = (float)heading[i];
				var thisScale = hasScale ? (float)scale[i] : 1f;
				var thisIsRound = hasRoundFlag ? (bool)isRound[i] : false;

				checkpointDefinitions.Add(new CheckpointDefinition(thisLocation, thisHeading, thisScale, thisIsRound));
				*/
				var newCheckpoint = new CheckpointDefinition();
				newCheckpoint.IsRound = hasRoundFlag ? (bool)isRound[i] : false;
				newCheckpoint.Location = location[i].ToVector3() + checkpointPositionOffset[newCheckpoint.IsRound ? "round" : "normal"];
				newCheckpoint.Heading = (float)heading[i];
				newCheckpoint.Scale = hasScale ? (float)scale[i] : 1f;

				checkpointDefinitions.Add(newCheckpoint);
			}

			return checkpointDefinitions;
		}
	}
}
