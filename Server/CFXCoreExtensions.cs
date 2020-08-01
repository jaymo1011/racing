using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Text;

using static CitizenFX.Core.Native.API;

namespace racing.Server
{
	public static class CFXCoreExtensions
	{
		public static void SetIntoVehicle(this Ped ped, Vehicle vehicle, int seat = -1)
		{
			SetPedIntoVehicle(ped.Handle, vehicle.Handle, seat);
		}
	}
}
