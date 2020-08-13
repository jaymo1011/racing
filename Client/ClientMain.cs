using CitizenFX.Core;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace racing.Client
{
	public class ClientMain : BaseScript
	{
		/*
		static object DefaultSpawnData = new {
			x = 2270.815f,
			y = 3756.9f,
			z = 38.5f,
			heading = 39.3f,
			model = GetHashKey("a_m_y_skater_02"),
		};
		*/
		
		static readonly object DefaultSpawnData = new {
			x = 345f,
			y = 4842f,
			z = -60f,
			heading = 39.3f,
			model = GetHashKey("a_m_y_skater_02"),
		};

		public static List<Prop> LoadedProps = new List<Prop>();

		// Kinda a dumb idea anyway
		//static readonly string FacilityIpl = "xm_x17dlc_int_placement_interior_33_x17dlc_int_02_milo_";
		//static readonly int FacilityInteriorId = 269313;
		//static Vector3 FacilityLocation = new Vector3(345f, 4842f, -60f);

		/*
		async Task LoadFacility()
		{
			Debug.WriteLine("loading IPL");

			// Lets load the facility!
			RequestIpl(FacilityIpl);
			SetFocusArea(FacilityLocation.X, FacilityLocation.Y, FacilityLocation.Z, 0f, 0f, 0f);
			while (!IsIplActive(FacilityIpl))
				await Delay(100);

			Debug.WriteLine("IPL has loaded!");

			// Maybe we'll copy / depend on bob74 IPL and expose interior customisation?
			// Not yet.
			EnableInteriorProp(FacilityInteriorId, "set_int_02_decal_09");
			RefreshInterior(FacilityInteriorId);

			EnableInteriorProp(FacilityInteriorId, "set_int_02_sleep3");
			SetInteriorPropColor(FacilityInteriorId, "set_int_02_sleep3", 9);
			RefreshInterior(FacilityInteriorId);

			EnableInteriorProp(FacilityInteriorId, "set_int_02_security");
			RefreshInterior(FacilityInteriorId);

			EnableInteriorProp(FacilityInteriorId, "set_int_02_cannon");
			SetInteriorPropColor(FacilityInteriorId, "set_int_02_cannon", 9);
			RefreshInterior(FacilityInteriorId);

			EnableInteriorProp(FacilityInteriorId, "set_int_02_shell");
			SetInteriorPropColor(FacilityInteriorId, "set_int_02_shell", 9);
			RefreshInterior(FacilityInteriorId);

			Debug.WriteLine("Props set, refreshing.");

			// Is interior ready is not viable, damn
			//while (!IsInteriorReady(FacilityInteriorId))
			//await Delay(100);

			Debug.WriteLine("Facility loading done, lets go!");

			ClearFocus();

			return;
		}
		*/

		static UGC.Map CurrentMap;

		public ClientMain()
		{
		
		}

		[EventHandler("onClientMapStart")]
		void OnClientMapStart()
		{
			// Make sure we don't automatically spawn on death, manual spawning only!
			Exports["spawnmanager"].setAutoSpawn(false);

			TriggerServerEvent("clientMapStarted");

			/*

			// Let's see if we can nab the UGC for the current map...
			string ugcFile = "none";

			if (GetNumResourceMetadata(mapName, "ugc_file") > 0)
			{
				// Found it!
				var ugcFilePath = GetResourceMetadata(mapName, "ugc_file", 0);
				ugcFile = LoadResourceFile(mapName, ugcFilePath); // We hope it's added as a "file"
			}
			else if (GetResourceMetadata(mapName, "isUgcUrlSurrogate", 0) == "yes")
			{
				// dunno what to do here!
				return;
			}
			else
			{
				// Not a valid race map, not sure what's going on really...
				return;
			}

			// Let's now load our new UGC stuff!
			//CurrentMap = ParseUGC(ugcFile);

			// Find where the loading scene (I think?!) is.
			/*Vector3? spawnVector = CurrentMap?.Get("race.scene").ToVector3();

			if (spawnVector != null)
			{
				var _spawnVector = (Vector3)spawnVector;
				// And now we spawn there
				Exports["spawnmanager"].spawnPlayer(new
				{
					x = _spawnVector.X,
					y = _spawnVector.Y,
					z = _spawnVector.Z,
					heading = 39.3f,
					model = GetHashKey("a_m_y_skater_02"),
				});
			}*/

			// Freeze the player so they don't fall through the world!


			// Load the actual map stuff!
			//PlacePropsFromUGC();
		}

		[EventHandler("racing:loadMissionJSON")]
		async void LoadMissionJson(string missionJson)
		{
			CurrentMap = new UGC.Map(missionJson);
			Debug.WriteLine("loading gayson");
			await CurrentMap.Load();
			Debug.WriteLine("done loading gayson");
			TriggerServerEvent("racing:onClientMapLoaded");
		}

		//[EventHandler("PlacePropsFromUGC")]
		async void PlacePropsFromUGC()
		{
			// early out, we don't deal with maps anymore!
			return;
			/*

			Debug.WriteLine("prop time!");
			Debug.WriteLine($"I guess we got ugc data? {JsonConvert.SerializeObject(CurrentMap.Mission["rule"])}");

			Debug.WriteLine("lets try and get definitions");
			List<PropDefinition> propDefinitions = GetPropDefinitions(CurrentMap.Prop);

			Debug.WriteLine("uhhh... lets go I guess???????");
			await propDefinitions.CreateProps();

			// So now thats done, let's tell the server we're all loaded and good to go for the race!
			TriggerServerEvent("racing:mapLoaded");*/
		}

		[EventHandler("debug_RegisterAllCheckpoints")]
		public void debug_RegisterAllCheckpoints(string checkpointDefinitionsJsonArray)
		{
			try
			{
				var cdja = JsonConvert.DeserializeObject<UGC.CheckpointDefinition[]>(checkpointDefinitionsJsonArray);
				cdja.All((cp) =>
				{
					UGC.CheckpointDefinition cpd = JsonConvert.DeserializeObject<UGC.CheckpointDefinition>(cp.ToString());
					cp.CreateCheckpoint();
					return true;
				});
			}
			catch (Exception e)
			{
				throw new ArgumentException("EventHandler was called with a JSON string which did not contain valid CheckpointDefinitions", "checkpointDefinitionsJsonArray", e);
			}

			Debug.WriteLine($"checkpoints are: {checkpointDefinitionsJsonArray}");
		}

		[Command("spawnnow")] // DEBUG
		void CommandSpawnNow()
		{
			Exports["spawnmanager"].spawnPlayer(DefaultSpawnData);
		}
	}
}
