--[[
	Vehicle Modification (server)

	The server side of vehicle modification handles mandating and notifying the client of what mods need to be applied to their vehicle.
	Only certain mods (ascetic mods) can be changed by the client but all mods must be applied by the client due to RPC not having the mod setters in at the moment.
	Once vehicle mod setters (and preferences) are available on server, the client component will likely not be needed.
]]

function EnsureRaceVehicleHasCorrectMods(vehicle)
	local vehicleHandle = vehicle.__data

	-- This is available on the server!
	if vehicle.state.VehicleModProfile.numberPlateText then
		SetVehicleNumberPlateText(vehicleHandle, vehicle.state.VehicleModProfile.numberPlateText)
	end

	TriggerClientEvent("racing:ensureVehicleMods", vehicle.state.OwningPlayer)
end
