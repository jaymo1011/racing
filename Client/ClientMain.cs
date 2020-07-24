using CitizenFX.Core;
using System;
using static CitizenFX.Core.Native.API;

namespace racing.Client
{
	public class ClientMain : BaseScript
	{
		static object DefaultSpawnData = new {
			x = 2270.815f,
			y = 3756.9f,
			z = 38.5f,
			heading = 39.3f,
			model = GetHashKey("a_m_y_skater_02"),
		};

		public ClientMain()
		{

		}

		[EventHandler("onClientMapStart")]
		void OnClientMapStart()
		{ 
			// Make sure we don't automatically spawn on death, manual spawning only!
			Exports["spawnmanager"].setAutoSpawn(false);
			Exports["spawnmanager"].spawnPlayer(DefaultSpawnData);
		}

		[Command("spawnnow")] // DEBUG
		void CommandSpawnNow()
		{
			Exports["spawnmanager"].spawnPlayer(DefaultSpawnData);
		}
	}
}
