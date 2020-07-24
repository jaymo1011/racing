using CitizenFX.Core;
using System;
using static CitizenFX.Core.Native.API;

namespace racing.Client
{
	public class ClientMain : BaseScript
	{
		public ClientMain()
		{

		}

		[EventHandler("onClientMapStart")]
		void OnClientMapStart()
		{
			Exports["spawnmanager"].setAutoSpawn(true);
			Exports["spawnmanager"].forceRespawn();
		}
	}
}
