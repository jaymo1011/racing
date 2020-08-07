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

			/*
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
			*/
		}

		public static List<PropDefinition> GetPropDefinitions(JObject pd)
		{
			// Fail real quick if we've been given invalid data
			if (!pd.ContainsKeys("no", "loc", "model", "head", "vRot"))
				throw new ArgumentException("Given prop data is missing required keys for props (no, loc, head, model, vRot)", "propData");

			// This is VERY ugly but, I'll fix it eventually
			// See, now its "fixed" but maybe I could automate this?
			var propDefinitions = new List<PropDefinition>();

			var numProps = (int)pd["no"];

			var location = pd.TryGetArray("loc");
			var rotation = pd.TryGetArray("vRot");
			var heading = pd.TryGetArray("head");
			var model = pd.TryGetArray("model");
			var textureVariant = pd.TryGetArray("prpclr", pd.TryGetArray("prpclc")); //var hasTextureVariant = (textureVariant != missingData);
			var lodDistance = pd.TryGetArray("prplod"); //var hasLodDistance = (lodDistance != missingData);
			var speedAdjustment = pd.TryGetArray("prpsba"); //var hasSpeedAdjustment = (speedAdjustment != missingData);

			for (int i = 0; i < numProps; i++)
			{
				PropDefinition prop = new PropDefinition();

				prop.Location =			(Vector3)	location?[i].ToVector3();
				prop.Heading =			(float)		heading?[i];
				prop.Model =			(int)		model?[i];
				prop.Rotation =			(Vector3)	rotation?[i].ToVector3();
				prop.EntityLODDist =	(int)		lodDistance?[i];
				prop.HasSpeedModifier =	(bool)		GetPropSpeedModificationParameters(prop.Model, (int)speedAdjustment?[i], out prop.SpeedAmount, out prop.SpeedDuration);
				prop.TextureVariant =	(int)		textureVariant?[i];

				propDefinitions.Add(prop);
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
			// bandito race vvar1 = 5f, 1.2f;
			//position += new Vector3(0f, 0f, 5f);
			var target = checkpointDef.Location; // No target for checkpoints justtt yet :)
			var radius = checkpointDef.IsRound ? 21f : 10f;
			var cylinderRadius = 100f;//checkpointDef.IsRound ? 10.5f : 5.5f;
			// Will probably be changed with other data
			var cylinderHeight = 9.5f;

			int checkpointHandle = CitizenFX.Core.Native.API.CreateCheckpoint(type, position.X, position.Y, position.Z, target.X, target.Y, target.Z, radius, CheckpointBaseColour[0], CheckpointBaseColour[1], CheckpointBaseColour[2], CheckpointBaseColour[3], 0);
			SetCheckpointCylinderHeight(checkpointHandle, cylinderHeight, cylinderHeight, cylinderRadius);
			SetCheckpointIconRgba(checkpointHandle, CheckpointConeColour[0], CheckpointConeColour[1], CheckpointConeColour[2], CheckpointConeColour[3]);
			if (!checkpointDef.IsRound)
			{
				float groundZ = World.GetGroundHeight(new Vector3(position.X, position.Y, position.Z + 1f));
				Vector3 planeOrigin = new Vector3(position.X, position.Y, groundZ);

				var ray = World.Raycast(new Vector3(planeOrigin.X, planeOrigin.Y, planeOrigin.Z + 1f), new Vector3(planeOrigin.X, planeOrigin.Y, planeOrigin.Z - 1f), IntersectOptions.Objects);
				
				if (ray.DitHit)
				{
					// yay our raycast hit!
					Debug.WriteLine($"got ray! {ray.HitPosition}::{ray.SurfaceNormal}");
					var vector1 = planeOrigin; //+ (Vector3.Up);
					//var vector2 = ray.HitPosition;// + ray.SurfaceNormal;
					// seems to be RENDER FROM X1 Y1 Z1 TO X2, Y2, Z2 so... yeah, invert it and it will **only** draw within the domain
					if (ray.SurfaceNormal.Z > 0f)
						N_0xf51d36185993515d(checkpointHandle, vector1.X - 0.05f, vector1.Y, vector1.Z, ray.SurfaceNormal.X, ray.SurfaceNormal.Y, ray.SurfaceNormal.Z);
				}

				// Blips still need to be managed!

				Debug.WriteLine($"created checkpoint {checkpointHandle} where the z is {groundZ}");
			}

			return new Checkpoint(checkpointHandle);
		}
	}
}
