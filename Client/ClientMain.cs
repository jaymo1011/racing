using CitizenFX.Core;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using System.Linq;
using static racing.UGC;
using Newtonsoft.Json;
using System.Collections.Generic;

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
		
		static object DefaultSpawnData = new {
			x = 345f,
			y = 4842f,
			z = -60f,
			heading = 39.3f,
			model = GetHashKey("a_m_y_skater_02"),
		};

		static string FacilityIpl = "xm_x17dlc_int_placement_interior_33_x17dlc_int_02_milo_";
		static int FacilityInteriorId = 269313;
		static Vector3 FacilityLocation = new Vector3(345f, 4842f, -60f);

		static UGCData CurrentMap;

		public ClientMain()
		{
		
		}

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

		[EventHandler("onClientMapStart")]
		async void OnClientMapStart()
		{
			Debug.WriteLine("mapstart!");

			// Make sure we don't automatically spawn on death, manual spawning only!
			Exports["spawnmanager"].setAutoSpawn(false);

			// Load the facility
			await LoadFacility();

			// And now we spawn there
			Exports["spawnmanager"].spawnPlayer(DefaultSpawnData);
		}

		[EventHandler("PlacePropsFromUGC")]
		async void PlacePropsFromUGC(string ugcData)
		{
			Debug.WriteLine("prop time!");

			CurrentMap = ParseUGC(ugcData);

			Debug.WriteLine($"I guess we got ugc data? {JsonConvert.SerializeObject(CurrentMap.Mission["rule"])}");

			Debug.WriteLine("lets try and get definitions");
			List<PropDefinition> propDefinitions = GetPropDefinitions(CurrentMap.Prop);

			Debug.WriteLine("uhhh... lets go I guess???????");
			await propDefinitions.CreateProps();
		}

		[Command("spawnnow")] // DEBUG
		void CommandSpawnNow()
		{
			Exports["spawnmanager"].spawnPlayer(DefaultSpawnData);
		}
	}
}
