using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace racing.Server
{
	/// <summary>
	/// Mimics the CitizenFX.Core.External.World library but only includes relevant things for racing.
	/// <para/>
	/// Could be added to clrcore at a later date!
	/// </summary>
	class World
	{
		/// <summary>
		/// Spawns a <see cref="Vehicle"/> of the given model at the position and heading specified.
		/// </summary>
		/// <param name="model">The model of the <see cref="Vehicle"/>.</param>
		/// <param name="position">The position to spawn the <see cref="Vehicle"/> at.</param>
		/// <param name="heading">The heading of the <see cref="Vehicle"/>.</param>
		/// <remarks>returns <c>null</c> if the <see cref="Vehicle"/> could not be spawned</remarks>
		public static Vehicle CreateVehicle(uint model, Vector3 position, float heading = 0f)
		{
			return new Vehicle(API.CreateVehicle(model, position.X, position.Y, position.Z, heading, true, false));
		}
		///<inheritdoc cref="CreateVehicle"/>
		public static Vehicle CreateVehicle(string model, Vector3 position, float heading = 0f)
		{
			return CreateVehicle((uint)API.GetHashKey(model), position, heading);
		}

		/// <summary>
		/// Spawns a <see cref="Prop"/> of the given model at the position specified.
		/// </summary>
		/// <param name="model">The model of the <see cref="Prop"/>.</param>
		/// <param name="position">The position to spawn the <see cref="Prop"/> at.</param>
		/// <param name="dynamic">if set to <c>true</c> the <see cref="Prop"/> will have physics; otherwise, it will be static.</param>
		/// <remarks>returns <c>null</c> if the <see cref="Prop"/> could not be spawned</remarks>
		public static Prop CreateProp(int model, Vector3 position, bool dynamic)
		{
			return new Prop(API.CreateObject(model, position.X, position.Y, position.Z, true, false, dynamic));
		}

		/// <summary>
		/// Spawns a <see cref="Prop"/> of the given model at the position specified without any offset.
		/// </summary>
		/// <param name="model">The model of the <see cref="Prop"/>.</param>
		/// <param name="position">The position to spawn the <see cref="Prop"/> at.</param>
		/// <param name="dynamic">if set to <c>true</c> the <see cref="Prop"/> will have physics; otherwise, it will be static.</param>
		/// <remarks>returns <c>null</c> if the <see cref="Prop"/> could not be spawned</remarks>
		public static Prop CreatePropNoOffset(uint model, Vector3 position, bool dynamic)
		{
			return new Prop(API.CreateObjectNoOffset(model, position.X, position.Y, position.Z, true, false, dynamic));
		}

		/// <summary>
		/// Spawns a <see cref="Ped"/> of the given model at the position and heading specified.
		/// </summary>
		/// <param name="model">The model of the <see cref="Ped"/>.</param>
		/// <param name="position">The position to spawn the <see cref="Ped"/> at.</param>
		/// <param name="heading">The heading of the <see cref="Ped"/>.</param>
		/// <remarks>returns <c>null</c> if the <see cref="Ped"/> could not be spawned</remarks>
		public static Ped CreatePed(uint model, Vector3 position, float heading = 0f)
		{
			return new Ped(API.CreatePed(26, model, position.X, position.Y, position.Z, heading, true, false));
		}
	}
}
