using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;

namespace racing.Server
{
	public class ServerMain : BaseScript
	{
		static readonly HttpClient httpClient = new HttpClient();

		/// <summary>
		/// The current map as a <see cref="UGC.Map"/> or, if there is no map loaded, <see langword="null" />.
		/// </summary>
		UGC.Map CurrentMap = null;
		string CurrentMapName = null;

		public ServerMain()
		{
			Debug.WriteLine("Setting up server state for racing...");

			Function.Call((Hash)(ulong)API.GetHashKey("SET_SYNC_ENTITY_LOCKDOWN_MODE"), "strict");

			MapManager.Instance.Value.RegisterKeyDirective("ugc_file");
			MapManager.Instance.Value.RegisterKeyDirective("ugc_data");

			// Add the connected tag to all players already connected to the server
			foreach (Player player in Players)
			{
				Debug.WriteLine($"{player.Name} has connected.");
				player.AddTag("connected");
			}
				

			//Debug.WriteLine("attempting some ugc parsing");
			//parsedRace = ParseUGC(UGCExample.json);
			//

			//Debug.WriteLine("attempting some checkpoint parsing");
			//var props = GetPropDefinitions(parsedRace.Prop);
			//Debug.WriteLine($"here's what we got :\n{JsonConvert.SerializeObject(props)}");
		}

		[EventHandler("onMapStart")]
		void OnMapStart(string mapName)
		{
			CurrentMapName = mapName;
		}

		[EventHandler("onServerResourceStart")]
		void OnServerResourceStart(string resourceName)
		{
			// Detect when the current map has started by using this event rather than onMapStart.
			// This ensures that map directives have been fully processed before we start looking for them.
			//string currentMapName = Exports["mapmanager"].getCurrentMap();
		
			if (CurrentMapName == resourceName)
				OnMapParsed(CurrentMapName);
		}

		// Yes, this will have an await for ugc_url stuff, just not right now!
		void OnMapParsed(string mapName)
		{
			string ugcJsonString = null;

			IEnumerable<dynamic> UGCDataEntries = MapManager.Instance.Value.GetDirectives("ugc_data");
			if (UGCDataEntries.Any())
			{
				foreach (string entry in UGCDataEntries)
				{
					ugcJsonString = entry;
					break;
				}
			}

			if (ugcJsonString == null)
			{
				var UGCFileEntries = MapManager.Instance.Value.GetDirectives("ugc_file");
				if (UGCFileEntries.Any())
				{
					foreach (string entry in UGCFileEntries)
					{
						var fileContent = API.LoadResourceFile(mapName, entry);
						if (fileContent != null)
						{
							ugcJsonString = fileContent;
							break;
						}
					}
				}
			}

			if (ugcJsonString != null)
			{
				CurrentMap = new UGC.Map(ugcJsonString);
				Debug.WriteLine($"MissionJSON sample: {JsonConvert.SerializeObject(CurrentMap.GetObject("mission.rule"))}");

				foreach (Player p in Players.WithTag("connected"))
				{
					p.AddTag("map:loading");
					p.TriggerEvent("racing:loadMissionJSON", CurrentMap.Json);
				}
			}
		}

		[EventHandler("onMapStop")]
		void OnMapStop()
		{
			foreach (Player p in Players.WithTag("connected"))
			{
				p.RemoveTag("map:loaded");
			}
		}

		[EventHandler("playerJoining")]
		void OnPlayerJoining([FromSource] Player source)
		{
			source.AddTag("connected");
			Debug.WriteLine($"{source.Name} has connected.");
		}

		[EventHandler("clientMapStarted")]
		void OnClientMapStarted([FromSource] Player source)
		{
			Debug.WriteLine($"{source.Name}'s map has started");

			// Other things we want to do on connect like send over the JSON
			if (CurrentMap != null)
			{
				source.AddTag("map:loading");
				source.TriggerEvent("racing:loadMissionJSON", CurrentMap.Json);
			}
				
		}

		[EventHandler("racing:onClientMapLoaded")]
		void OnClientMapLoaded([FromSource] Player source)
		{
			Debug.WriteLine($"{source.Name} has placed props");
			// Other things we want to do on connect like send over the JSON
			source.RemoveTag("map:loading");
			source.AddTag("map:loaded");
			//source.TriggerEvent("racing:");
		}

		[EventHandler("onResourceStop")]
		void OnResourceStop(string resourceName)
		{
			if (API.GetCurrentResourceName() != resourceName) return;

			Debug.WriteLine("Reverting server state...");

			// If there was a getter for the entity lockdown state, we could save the actual state on startup and set it here.
			// Currently there isn't but almost nothing uses entity lockdown so it's best to just set it inactive.
			Function.Call((Hash)API.GetHashKey("SET_SYNC_ENTITY_LOCKDOWN_MODE"), "inactive");
		}

		// DEBUG
		// Just a debug command for testing out server-side vehicle creation and other code stuff.
		[Command("vehiclepls", Restricted = false)]
		void GivePlayerVehicle(Player caller, string[] args)
		{
			if (args.Length <= 0)
			{
				caller.TriggerEvent("chat:addMessage", new
				{
					color = new[] { 255, 255, 0 },
					args = new[] { "Vehicle Spawner", $"You must tell me the model of the vehicle you want!" }
				});

				return;
			}

			string model = args[0];
			Vector3 playerPos = caller.Character.Position;	
			
			var veh = World.CreateVehicle(model, playerPos, caller.Character.Heading);
			caller.Character.SetIntoVehicle(veh);

			caller.TriggerEvent("chat:addMessage", new
			{
				color = new[] { 255, 255, 0 },
				args = new[] { "Vehicle Spawner", $"Spawned a {model} at {playerPos}. It's handle is {veh.Handle} and you should now be in it." }
			});
		}

		// DEBUG
		// lets start doing actual race things?!
		[Command("startRace")]
		void DoTestRaceSetup()
		{
			try
			{
				//List<Vector3> spawnLocations = (List<Vector3>)((List<Object>)CurrentMap?["veh"]?["loc"]?["cuck cuck cuck uck"]).Cast<Vector3>();
				//List<float> spawnHeadings = (List<float>)((List<object>)CurrentMap?["race"]?["veh"]?["head"]).Cast<float>();
				Debug.WriteLine("LETS RACE");
				var spawnLocations = CurrentMap.GetList<Vector3>("mission.veh.loc");
				var spawnHeadings = CurrentMap.GetList<float>("mission.veh.head");
				//List<UGC.CheckpointDefinition> checkpointDefinitions = CurrentMap.GetCheckpointDefinitions();
				//string checkpointString = JArray.FromObject(checkpointDefinitions).ToString();
				int plyCount = 0;
				// Need to make a vehicle for all players and then set them into it
				foreach (Player player in Players)
				{
					Debug.WriteLine($"cmon, {player.Name}, lets do this!");
					//player.TriggerEvent("PlacePropsFromUGC", UGCExample.json);
					//player.TriggerEvent("debug_RegisterAllCheckpoints", checkpointString);
					var veh = World.CreateVehicle("nero", spawnLocations[plyCount], 0f); // Everyone gets a nero!
					API.FreezeEntityPosition(veh.Handle, true);
					player.Character.SetIntoVehicle(veh);
					plyCount++;
				}
			} 
			catch (Exception e)
			{
				Debug.WriteLine(e.ToString());
			}
		}

		[Command("unfreezeme")]
		void UnfreezeCommand(Player caller)
		{
			if (API.GetVehiclePedIsIn(caller.Character.Handle, false) != 0)
				API.FreezeEntityPosition(API.GetVehiclePedIsIn(caller.Character.Handle, false), false);
		}


		/*
		[EventHandler("onMapStart")]
		async void OnMapStart(string mapName)
		{
			string ugcFile = "none";

			if (API.GetNumResourceMetadata(mapName, "ugc_file") > 0)
			{
				var ugcFilePath = API.GetResourceMetadata(mapName, "ugc_file", 0);
				ugcFile = API.LoadResourceFile(mapName, ugcFilePath);
			}
			/*
			else if (API.GetResourceMetadata(mapName, "isUgcUrlSurrogate", 0) != null)
			{
				// we haven't implemented getting stuff from url yet
				// will require some client state sending whaterys (or maybe not?)
				try
				{
					ugcFile = await httpClient.GetStringAsync("http://prod.cloud.rockstargames.com/ugc/gta5mission/0000/vyTABS5xR06--t_w6e9t0w/0_0_en.json");
				}
				catch (HttpRequestException e)
				{
					Debug.WriteLine("Exception while requesting a UGC URL!\nMessage: {0}", e.Message);
				}
				return;
			}
			
			else
			{	
				// no map loading today!
				return;
			}

			if (ugcFile != "none")
			{
				Debug.WriteLine($"heres some of the UGC we got {ugcFile.Substring(0, 32)}");
				Debug.WriteLine("attempting some ugc parsing");
				CurrentMap = ParseUGC(ugcFile);
				Debug.WriteLine($"here's what we got :\n{JsonConvert.SerializeObject(CurrentMap.Mission["rule"])}");
			}
		}
		*/
	}
}
