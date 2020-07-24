using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace racing.Server
{
	public class ServerMain : BaseScript
	{
		public ServerMain()
		{
			Debug.WriteLine("Setting up server state for racing...");

			Function.Call((Hash)(ulong)API.GetHashKey("SET_SYNC_ENTITY_LOCKDOWN_MODE"), "strict");
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
			caller.SetIntoVehicle(veh);

			caller.TriggerEvent("chat:addMessage", new
			{
				color = new[] { 255, 255, 0 },
				args = new[] { "Vehicle Spawner", $"Spawned a {model} at {playerPos}. It's handle is {veh.Handle} and you should now be in it." }
			});
		}
	}
}
