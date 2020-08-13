using CitizenFX.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using static CitizenFX.Core.Native.API;

namespace racing
{
	public static class HudColour
	{
		public static Color Get(int hudColourIndex)
		{
			int r = 0;
			int g = 0;
			int b = 0;
			int a = 0;
			GetHudColour(hudColourIndex, ref r, ref g, ref b, ref a);
			return Color.FromArgb(a, r, g, b);
		}
	}

	public static partial class UGC
	{
		//public static int[] CheckpointBaseColour = { 254, 235, 169, 255 }; //GetHudColour(13);
		//public static int[] CheckpointConeColour = { 93, 182, 229, 255 }; //GetHudColour(134);
		public static Color CheckpointBaseColour = HudColour.Get(13);
		public static Color CheckpointConeColour = HudColour.Get(134);

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

		public static List<PropDefinition> GetPropDefinitions(this Map map)
		{
			if (!map.GetObject("mission.prop").ContainsKeys("no", "loc", "model", "head", "vRot"))
				throw new ArgumentException("The current map is missing required keys for props (no, loc, head, model, vRot)", "map");

			List<PropDefinition> props = new List<PropDefinition>();

			int numProps = map.Get<int>("mission.prop.no");

			List<Vector3> location = map.GetList<Vector3>("mission.prop.loc");
			List<Vector3> rotation = map.GetList<Vector3>("mission.prop.vRot");
			List<float> heading = map.GetList<float>("mission.prop.head");
			List<int> model = map.GetList<int>("mission.prop.model");

			List<int> prpclc = map.GetList<int>("mission.prop.prpclc");
			List<int> prpclr = map.GetList<int>("mission.prop.prpclr");
			List<int> textureVariation = prpclc != null ? prpclc : prpclr;
			List<int> lodDistance = map.GetList<int>("mission.prop.prplod");
			List<int> speedAdjust = map.GetList<int>("mission.prop.prpsba");

			for (int i = 0; i < numProps; i++)
			{
				PropDefinition prop = new PropDefinition();

				prop.Location = location[i];
				prop.Heading = heading[i];
				prop.Model = model[i];
				prop.Rotation = rotation[i];

				prop.EntityLODDist = (int)(lodDistance?[i]);
				prop.HasSpeedModifier = GetPropSpeedModificationParameters(prop.Model, (int)(speedAdjust?[i]), out prop.SpeedAmount, out prop.SpeedDuration);
				prop.TextureVariant = (int)(textureVariation?[i]);

				props.Add(prop);
			}

			return props;
		}

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
		//}

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

			int checkpointHandle = CitizenFX.Core.Native.API.CreateCheckpoint(type, position.X, position.Y, position.Z, target.X, target.Y, target.Z, radius, CheckpointBaseColour.R, CheckpointBaseColour.G, CheckpointBaseColour.B, CheckpointBaseColour.A, 0);
			SetCheckpointCylinderHeight(checkpointHandle, cylinderHeight, cylinderHeight, cylinderRadius);
			SetCheckpointIconRgba(checkpointHandle, CheckpointConeColour.R, CheckpointConeColour.G, CheckpointConeColour.B, CheckpointConeColour.A);
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

		public static async Task Load(this Map map)
		{
			// Generate all new props (first, in case of errors and stuff)
			List<PropDefinition> props = map.GetPropDefinitions();

			// Delete all old props
			foreach (Prop prop in Client.ClientMain.LoadedProps)
				prop.Delete();

			// Create all new props
			Client.ClientMain.LoadedProps = await CreateProps(props);
		}
	}
}
