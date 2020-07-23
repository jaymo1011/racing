using CitizenFX.Core;
using System;

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
