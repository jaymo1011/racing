local function ApplyVehiclePrimaryColourProfile(veh, colour)
	if colour.custom then
		SetVehicleCustomPrimaryColour(veh, colour.custom.r, colour.custom.g, colour.custom.b)
	else
		SetVehicleModColor_1(veh, colour.type, colour.value, 0)
	end
end

local function ApplyVehicleSecondaryColourProfile(veh, colour)
	if colour.isCustom then
		SetVehicleCustomSecondaryColour(veh, colour.r, colour.g, colour.b)
	else
		SetVehicleModColor_2(veh, colour.type, colour.value)
	end
end

local function ApplyVehicleModificationProfile(veh, mods)
	-- Verify that the mods apply to this vehicle model
	if mods.vehicleModel and GetEntityModel(veh) ~= mods.vehicleModel then return end 

	-- Set ModKit
	SetVehicleModKit(veh, mods.ModKit or 0)

	-- Set vehicle colours
	if mods.colour then
		local colourMod = mods.colour

		if colourMod.primary then
			ApplyVehiclePrimaryColourProfile(veh, colourMod.primary)
		end

		if colourMod.secondary then
			ApplyVehicleSecondaryColourProfile(veh, colourMod.primary)
		end

		SetVehicleExtraColours(veh, colourMod.pearl or 0, colourMod.wheel or 0)

		if colourMod.dash then
			SetVehicleDashboardColour(veh, colourMod.dash)
		end

		if colourMod.interior then
			SetVehicleInteriorColour(veh, colourMod.interior)
		end

		if colourMod.neon then
			local colour = colourMod.neon
			SetVehicleNeonLightsColour(veh, colour.r, colour.g, colour.b)
		end

		if colourMod.xenon then
			SetVehicleXenonLightsColour(veh, colourMod.xenon)
		end
	end
	
	-- Save the custom tires bool to save on indexing
	local hasCustomTires = mods.customTires or false

	-- Set all mods which use SET_VEHICLE_MOD
	for _, vehicleMod in ipairs(mods.vehicleMods) do
		SetVehicleMod(veh, vehicleMod.type, vehicleMod.index, hasCustomTires)
	end
end

RegisterNetEvent("racing:ensureVehicleMods")
AddEventHandler("racing:ensureVehicleMods", function()
	-- Get and ensure our race vehicle
	local vehicleNetId = LocalPlayer.state.RaceVehicleNetworkId or false
	if not vehicleNetId then
		printf("The server told us to set vehicle mods when we don't have a race vehicle!")
		return
	end

	local vehicleHandle = NetworkGetEntityFromNetworkId(vehicleNetId) or 0
	if vehicleHandle == 0 or not DoesEntityExist(vehicleHandle) then
		printf("The server told us to set vehicle mods on a vehicle that we don't know about!")
		return
	end

	local vehicle = Entity(vehicleHandle)

	if vehicle.state.OwningPlayer ~= GetPlayerServerId(PlayerId()) then
		printf("The server told us to set vehicle mods on a vehicle that we don't own! (%s ~= %s)", vehicle.state.OwningPlayer, PlayerId())
		return
	end

	if not vehicle.state.VehicleModProfile then
		printf("The server told us to set vehicle mods on a vehicle without a modification profile!")
		return
	end

	printf("Setting vehicle mods!")

	-- Apply the mod profile!
	ApplyVehicleModificationProfile(vehicleHandle, vehicle.state.VehicleModProfile)
end)
