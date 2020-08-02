using CitizenFX.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using static CitizenFX.Core.Native.API;

namespace racing
{
	public static partial class UGC
	{
		public static int[] CheckpointBaseColour = { 254, 235, 169, 255 }; //GetHudColour(13);
		public static int[] CheckpointConeColour = { 93, 182, 229, 255 }; //GetHudColour(134);

		static bool GetPropSpeedModificationParameters(int model, int prpsba, out int speedUp, out float duration)
		{
			// Oh you are going to hate me.

			// fast out if prpsba is -1
			if (prpsba == -1)
			{
				speedUp = -1;
				duration = -1;
				return false;
			}

			// This is r*'s fault!
			switch (model)
			{
				case -1006978322:
				case -388593496:
				case -66244843:
				case -1170462683:
				case 993442923:
				case 737005456:
				case -904856315:
				case -279848256:
				case 588352126:
					switch (prpsba)
					{
						case 1:
							speedUp = 15;
							duration = 0.3f;
							return true;

						case 2:
							speedUp = 25;
							duration = 0.3f;
							return true;

						case 3:
							speedUp = 35;
							duration = 0.5f;
							return true;

						case 4:
							speedUp = 45;
							duration = 0.5f;
							return true;

						case 5:
							speedUp = 100;
							duration = 0.5f;
							return true;

						default:
							speedUp = 25;
							duration = 0.4f;
							return true;
					}

				case 346059280:
				case 620582592:
				case 85342060:
				case 483832101:
				case 930976262:
				case 1677872320:
				case 708828172:
				case 950795200:
				case -1260656854:
				case -1875404158:
				case -864804458:
				case -1302470386:
				case 1518201148:
				case 384852939:
				case 117169896:
				case -1479958115:
				case -227275508:
				case 1431235846:
				case 1832852758:
					duration = -1f;
					switch (prpsba)
					{
						case 1:
							speedUp = 44;
							return true;

						case 2:
							speedUp = 30;
							return true;

						case 3:
							speedUp = 16;
							return true;

						default:
							speedUp = 30;
							return true;
					}

				default:
					speedUp = -1;
					duration = -1f;
					return false;
			}
		}

		public struct PropDefinition
		{
			public float Heading;
			public Vector3 Location;
			public int Model;
			public Vector3 Rotation;
			public int EntityLODDist;
			public bool HasSpeedModifier;
			public int SpeedAmount;
			public float SpeedDuration;
			public int TextureVariant;

			public PropDefinition(Vector3 l, Vector3 r, float h, int m, int texVariant, int prplod, int prpsba)
			{
				Heading = h;
				Location = l;
				Model = m;
				Rotation = r;
				EntityLODDist = prplod;
				HasSpeedModifier = GetPropSpeedModificationParameters(m, prpsba, out SpeedAmount, out SpeedDuration);
				TextureVariant = texVariant;
			}
		}

		public static List<PropDefinition> GetPropDefinitions(JObject pd)
		{
			// Fail real quick if we've been given invalid data
			if (!pd.ContainsKeys("no", "loc", "model", "head", "vRot"))
				throw new ArgumentException("Given prop data is missing required keys!", "propData");

			// This is VERY ugly but, I'll fix it eventually
			var propDefinitions = new List<PropDefinition>();
			var numProps = (int)pd["no"];
			var locations = (JArray)pd["loc"];
			var rotations = (JArray)pd["vRot"];
			var headings = (JArray)pd["head"];
			var models = (JArray)pd["model"];
			var textureVariants = pd.ContainsKey("prpclr") ? (JArray)pd["prpclr"] : (JArray)pd["prpclc"];
			JArray lodDistances = new JArray(); var hasLodDistances = pd.ContainsKey("prplod"); if (hasLodDistances)
				lodDistances = (JArray)pd["prplod"];
			JArray propSpeeds = new JArray(); var hasPropSpeeds = pd.ContainsKey("prplod"); if (hasPropSpeeds)
				propSpeeds = (JArray)pd["prpsba"];

			for (int i = 0; i < numProps; i++)
			{
				int lodDist = -1;
				if (hasLodDistances)
					lodDist = (int)lodDistances[i];
				int prpsba = -1;

				if (hasPropSpeeds)
					prpsba = (int)propSpeeds[i];

				propDefinitions.Add(new PropDefinition(locations[i].ToVector3(), rotations[i].ToVector3(), (float)headings[i], (int)models[i], (int)textureVariants[i], lodDist, prpsba));
			}

			return propDefinitions;
		}

		public static async Task<List<Prop>> CreateProps(this List<PropDefinition> propDefinitions)
		{
			var propList = new List<Prop>();

			foreach (PropDefinition propDefinition in propDefinitions)
			{
				// Lets make a prop!
				var model = new Model(propDefinition.Model);
				if (!await model.Request(2000))
					continue;

				var newProp = new Prop(CreateObjectNoOffset((uint)model.Hash, propDefinition.Location.X, propDefinition.Location.Y, propDefinition.Location.Z, false, true, false));
				FreezeEntityPosition(newProp.Handle, true);
				newProp.Heading = propDefinition.Heading;
				//newProp.Rotation = propDefinition.Rotation;
				SetEntityRotation(newProp.Handle, propDefinition.Rotation.X, propDefinition.Rotation.Y, propDefinition.Rotation.Z, 2, false);

				if (propDefinition.TextureVariant > -1)
					SetObjectTextureVariant(newProp.Handle, propDefinition.TextureVariant);
				if (propDefinition.EntityLODDist > -1)
					SetEntityLodDist(newProp.Handle, propDefinition.EntityLODDist);
				if (propDefinition.HasSpeedModifier)
				{
					if (propDefinition.SpeedAmount > -1)
						SetObjectStuntPropSpeedup(newProp.Handle, propDefinition.SpeedAmount);
					if (propDefinition.SpeedDuration > -1)
						SetObjectStuntPropDuration(newProp.Handle, propDefinition.SpeedDuration);
				}
			}

			return propList;
		}

		public static Checkpoint CreateCheckpoint(this CheckpointDefinition checkpointDef)
		{
			var type = checkpointDef.IsRound ? 10 : 5;
			var position = checkpointDef.Location;
			var target = checkpointDef.Location; // No target for checkpoints justtt yet :)
			var radius = checkpointDef.IsRound ? 10.5f : 5.5f;
			var cylinderRadius = checkpointDef.IsRound ? 10.5f : 5.5f;
			// Will probably be changed with other data
			var cylinderHeight = 16f;

			int checkpointHandle = CitizenFX.Core.Native.API.CreateCheckpoint(type, position.X, position.Y, position.Z, target.X, target.Y, target.Z, radius, CheckpointBaseColour[0], CheckpointBaseColour[1], CheckpointBaseColour[2], CheckpointBaseColour[3], 0);
			SetCheckpointCylinderHeight(checkpointHandle, cylinderHeight, cylinderHeight, cylinderRadius);
			SetCheckpointIconRgba(checkpointHandle, CheckpointConeColour[0], CheckpointConeColour[1], CheckpointConeColour[2], CheckpointConeColour[3]);

			// Blips still need to be managed!

			return new Checkpoint(checkpointHandle);
		}
	}
}
