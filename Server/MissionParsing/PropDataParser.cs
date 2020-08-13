using CitizenFX.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static racing.UGC;

namespace racing.Server.MissionParsing
{
	public static class PropParser
	{
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
	}
}
