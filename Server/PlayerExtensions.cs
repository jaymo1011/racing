using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Text;

using static CitizenFX.Core.Native.API;

namespace racing.Server
{
	public static class PlayerExtensions
	{
		public static void SetIntoVehicle(this Player player, Vehicle vehicle, int seat = -1)
		{
			SetPedIntoVehicle(player.Character.Handle, vehicle.Handle, seat);
		}
	}
}
